using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using PipeType = TangledPipePiece.PipeType;

public class PuzzleManager : MonoBehaviour
{
    [Header("Original Interactable Objects")]
    [SerializeField] private GameObject Tape;              // has collider2d, holdable, bind position to mouse position when held
    [SerializeField] private GameObject BrokenTubeL;        // has collider2d, drag tape to this to fix
    [SerializeField] private GameObject BrokenTubeR;        // solve tangled tube puzzle to fix
    [SerializeField] private GameObject Handle;             // has collider2d, holdable
    [SerializeField] private GameObject IncompleteTable;     // has collider2d, drag handle here once both tubes are fixed
    [SerializeField] private GameObject DeadBird;            // replaced with BirdStairs after handle puzzle is solved
    [SerializeField] private GameObject TangledTubeButton;   // collider2D on an Empty -> opens TangledTubeScreen
    [SerializeField] private GameObject ClueButton;          // will show PuzzleClue for the HandlePuzzle
    [SerializeField] private Sprite PuzzleClue;
    [SerializeField] private GameObject Background;          // disabled along with everything else while a puzzle screen is open

    [Header("Puzzle Screens")]
    [SerializeField] private GameObject TangledTubeScreen;   // full screen puzzle, disabled on load
    [SerializeField] private GameObject TangledTubePieces;   // parent object; children are the 5x5 grid of pieces, row-major order

    [SerializeField] private GameObject HandlePuzzle;        // full screen puzzle, disabled on load
    [SerializeField] private GameObject HandleRot;           // drag to rotate clockwise; full turn solves the puzzle

    [Header("Repaired Objects")]
    [SerializeField] private Sprite FixedTubeL;
    [SerializeField] private Sprite FixedTubeR;
    [SerializeField] private Sprite FixedHandleTable;
    [SerializeField] private Sprite BirdStairs;

    [Header("Cursor")]
    [SerializeField] private Texture2D CursorHandPoint; // texture must have Read/Write Enabled in its import settings
    [SerializeField] private Vector2 cursorHotspot = Vector2.zero;
    private int hoverCount = 0;

    [Header("Tangled Tube Puzzle Settings")]
    [SerializeField] private int gridSize = 5;

    private int startExternalDir;
    private int endExternalDir;

    [Header("Debug")]
    [SerializeField] private bool debugTangledTube = false;
    [SerializeField] private KeyCode debugLogKey = KeyCode.F9;

    // Which direction (N/E/S/W) is open for each pipe shape at 0 turns.
    // Index: 0 = North, 1 = East, 2 = South, 3 = West (clockwise order).
    private static readonly Dictionary<PipeType, int[]> BaseOpenings = new Dictionary<PipeType, int[]>
{
        { PipeType.Straight, new[] { 0, 2 } },    // N & S
        { PipeType.Elbow,    new[] { 0, 3 } },    // N & W
        { PipeType.TSection, new[] { 1, 2, 3 } }, // E & S & W
    };

    private static readonly int[] RowDelta = { -1, 0, 1, 0 }; // N, E, S, W
    private static readonly int[] ColDelta = { 0, 1, 0, -1 };

    private struct PieceData
    {
        public Transform transform;
        public PipeType type;
        public int currentTurns; // 0-3, 0 = original rotation
    }

    private PieceData[,] tangledPieces;
    private Vector2Int startCell;
    private Vector2Int endCell;

    private enum HeldItem { None, Tape, Handle }
    private HeldItem heldItem = HeldItem.None;
    private Transform heldTransform;

    private Vector3 tapeOriginalPos;
    private Vector3 handleOriginalPos;
    private Transform handleOriginalParent;

    private bool tubeLFixed = false;
    private bool tubeRFixed = false;
    private bool handlePuzzleSolved = false;

    private bool trackingHandleRotation = false;
    private float accumulatedRotation = 0f;
    private Vector3 lastMouseDir;

    // Objects that make up the "main room" view, hidden while a full screen puzzle is open.
    private GameObject[] mainSceneObjects;

    void Start()
    {
        tapeOriginalPos = Tape.transform.position;
        handleOriginalPos = Handle.transform.position;
        handleOriginalParent = Handle.transform.parent;

        TangledTubeScreen.SetActive(false);
        TangledTubePieces.SetActive(false);
        HandlePuzzle.SetActive(false);
        HandleRot.SetActive(false);

        InitTangledTubeGrid();

        mainSceneObjects = new GameObject[]
        {
            Tape, BrokenTubeL, BrokenTubeR, Handle, IncompleteTable,
            DeadBird, TangledTubeButton, ClueButton, Background
        };
    }

    void Update()
    {
        if (heldItem != HeldItem.None && heldTransform != null)
        {
            Vector3 mouseWorldPos = GetMouseWorldPos();
            mouseWorldPos.z = heldTransform.position.z;
            heldTransform.position = mouseWorldPos;

            if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                HandleDrop(mouseWorldPos);
            }
        }

        if (trackingHandleRotation)
        {
            if (Mouse.current.leftButton.isPressed)
            {
                UpdateHandleRotation();
            }
            else
            {
                trackingHandleRotation = false;
            }
        }

        if (debugTangledTube && Keyboard.current != null && Keyboard.current[Key.F9].wasPressedThisFrame)
        {
            DebugLogTangledGrid();
        }
    }
    private void DebugLogTangledGrid()
    {
        string[] dirNames = { "N", "E", "S", "W" };

        Debug.Log("=== Tangled Tube Grid Debug ===");
        for (int r = 0; r < gridSize; r++)
        {
            string row = "";
            for (int c = 0; c < gridSize; c++)
            {
                PieceData p = tangledPieces[r, c];
                bool[] open = GetOpenSides(p);
                string openStr = "";
                for (int d = 0; d < 4; d++)
                    if (open[d]) openStr += dirNames[d];

                string tag = "";
                if (new Vector2Int(r, c) == startCell) tag = "[START]";
                if (new Vector2Int(r, c) == endCell) tag = "[END]";

                row += $"({r},{c}) {p.type} z={p.transform.localEulerAngles.z:F0} turns={p.currentTurns} open={openStr} {tag} | ";
            }
            Debug.Log(row);
        }

        // Start/end external-facing check
        bool[] startOpen = GetOpenSides(tangledPieces[startCell.x, startCell.y]);
        bool[] endOpen = GetOpenSides(tangledPieces[endCell.x, endCell.y]);
        Debug.Log($"Start external dir={dirNames[startExternalDir]} open={startOpen[startExternalDir]}");
        Debug.Log($"End external dir={dirNames[endExternalDir]} open={endOpen[endExternalDir]}");

        Debug.Log($"IsPathConnected() = {IsPathConnected()}");
    }

    private Vector3 GetMouseWorldPos()
    {
        Vector2 screenPos = Mouse.current.position.ReadValue();
        return Camera.main.ScreenToWorldPoint(screenPos);
    }

    // ---------------------------------------------------------------
    // Holding / dragging Tape and Handle
    // ---------------------------------------------------------------

    public void OnTapeClicked()
    {
        if (heldItem == HeldItem.Tape) return;
        ReturnHeldItemToOriginalSpot();
        heldItem = HeldItem.Tape;
        heldTransform = Tape.transform;
    }

    public void OnHandleClicked()
    {
        if (heldItem == HeldItem.Handle) return;
        ReturnHeldItemToOriginalSpot();
        heldItem = HeldItem.Handle;
        heldTransform = Handle.transform;
    }

    private void ReturnHeldItemToOriginalSpot()
    {
        if (heldItem == HeldItem.Tape)
        {
            Tape.transform.position = tapeOriginalPos;
        }
        else if (heldItem == HeldItem.Handle)
        {
            Handle.transform.position = handleOriginalPos;
        }

        heldItem = HeldItem.None;
        heldTransform = null;
    }

    private void HandleDrop(Vector3 worldPos)
    {
        Collider2D[] hits = Physics2D.OverlapPointAll(worldPos);
        GameObject target = null;

        foreach (Collider2D hit in hits)
        {
            if (hit.gameObject != heldTransform.gameObject)
            {
                target = hit.gameObject;
                break;
            }
        }

        if (target != null)
            OnDropOnTarget(target);
        else
            ReturnHeldItemToOriginalSpot();
    }

    private void OnDropOnTarget(GameObject target)
    {
        if (target == BrokenTubeL && heldItem == HeldItem.Tape)
        {
            FixTubeL();
        }
        else if (target == IncompleteTable && heldItem == HeldItem.Handle)
        {
            if (tubeLFixed && tubeRFixed)
                OpenHandlePuzzle();
            else
                ReturnHeldItemToOriginalSpot();
        }
        else
        {
            ReturnHeldItemToOriginalSpot();
        }
    }

    // ---------------------------------------------------------------
    // Broken Tube L
    // ---------------------------------------------------------------

    private void FixTubeL()
    {
        heldItem = HeldItem.None;
        heldTransform = null;

        SpriteRenderer sr = BrokenTubeL.GetComponent<SpriteRenderer>();
        if (sr != null) sr.sprite = FixedTubeL;

        Destroy(Tape);
        tubeLFixed = true;
    }

    // ---------------------------------------------------------------
    // Tangled Tube puzzle (fixes Broken Tube R)
    // ---------------------------------------------------------------

    public void OnTangledTubeButtonClicked()
    {
        SetMainSceneActive(false);
        TangledTubeScreen.SetActive(true);
        TangledTubePieces.SetActive(true);
    }

    private void InitTangledTubeGrid()
    {
        tangledPieces = new PieceData[gridSize, gridSize];

        // Assumes TangledTubePieces' children are laid out row-major
        // (row 0 col 0, row 0 col 1, ... row 0 col 4, row 1 col 0, ...).
        // If your hierarchy order doesn't match the visual grid, reorder
        // the children in the scene, or swap this to read grid position
        // from each child's local position instead.
        int childIndex = 0;
        for (int r = 0; r < gridSize; r++)
        {
            for (int c = 0; c < gridSize; c++)
            {
                if (childIndex >= TangledTubePieces.transform.childCount) continue;

                Transform piece = TangledTubePieces.transform.GetChild(childIndex);
                TangledPipePiece pipeInfo = piece.GetComponent<TangledPipePiece>();

                if (pipeInfo == null)
                {
                    Debug.LogWarning($"Tangled tube piece at ({r},{c}) [{piece.name}] has no TangledPipePiece component.");
                }
                else
                {
                    if (pipeInfo.IsStart) { startCell = new Vector2Int(r, c); startExternalDir = pipeInfo.ExternalDirIndex; }
                    if (pipeInfo.IsEnd) { endCell = new Vector2Int(r, c); endExternalDir = pipeInfo.ExternalDirIndex; }
                }

                tangledPieces[r, c] = new PieceData
                {
                    transform = piece,
                    type = pipeInfo != null ? pipeInfo.Type : PipeType.Straight,
                    currentTurns = GetTurnsFromTransform(piece)
                };
                childIndex++;
            }
        }
    }

    // Reads how many 90-degree clockwise turns a piece is already rotated by in
    // the scene (pieces are allowed to start pre-scrambled), so the internal
    // turn-count stays in sync with what's actually shown from the very start.
    // Assumes pieces are only ever rotated in 90-degree steps.
    private int GetTurnsFromTransform(Transform piece)
    {
        float zAngle = piece.localEulerAngles.z;
        int turns = Mathf.RoundToInt(-zAngle / 90f) % 4;
        return (turns + 4) % 4;
    }

    public void OnTangledPieceClicked(Transform pieceTransform)
    {
        for (int r = 0; r < gridSize; r++)
        {
            for (int c = 0; c < gridSize; c++)
            {
                if (tangledPieces[r, c].transform == pieceTransform)
                {
                    RotatePiece(r, c);
                    return;
                }
            }
        }
    }

    private void RotatePiece(int row, int col)
    {
        PieceData piece = tangledPieces[row, col];
        piece.currentTurns = (piece.currentTurns + 1) % 4;
        piece.transform.Rotate(0f, 0f, -90f); // negative = clockwise on screen; flip sign if yours spins the wrong way
        tangledPieces[row, col] = piece;

        if (debugTangledTube) DebugLogTangledGrid();

        CheckTangledTubeSolved();
    }

    private void CheckTangledTubeSolved()
    {
        if (IsPathConnected())
        {
            CompleteTangledTubePuzzle();
        }
    }

    // Returns which of N/E/S/W (index 0-3) are open for a piece, accounting for its rotation.
    private bool[] GetOpenSides(PieceData piece)
    {
        bool[] sides = new bool[4];
        if (BaseOpenings.TryGetValue(piece.type, out int[] baseDirs))
        {
            foreach (int dir in baseDirs)
            {
                sides[(dir + piece.currentTurns) % 4] = true;
            }
        }
        return sides;
    }

    // Flood-fills through pieces whose open sides line up with their neighbor's
    // opposite side, starting from startCell. Returns true if endCell is reached.
    private bool IsPathConnected()
    {
        bool[] startOpen = GetOpenSides(tangledPieces[startCell.x, startCell.y]);
        if (!startOpen[startExternalDir]) return false;

        bool[] endOpen = GetOpenSides(tangledPieces[endCell.x, endCell.y]);
        if (!endOpen[endExternalDir]) return false;

        bool[,] visited = new bool[gridSize, gridSize];
        Queue<Vector2Int> queue = new Queue<Vector2Int>();

        queue.Enqueue(startCell);
        visited[startCell.x, startCell.y] = true;

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            if (current == endCell) return true;

            bool[] openSides = GetOpenSides(tangledPieces[current.x, current.y]);

            for (int dir = 0; dir < 4; dir++)
            {
                if (!openSides[dir]) continue;

                int nRow = current.x + RowDelta[dir];
                int nCol = current.y + ColDelta[dir];
                if (nRow < 0 || nRow >= gridSize || nCol < 0 || nCol >= gridSize) continue;
                if (visited[nRow, nCol]) continue;

                int oppositeDir = (dir + 2) % 4;
                bool[] neighborOpenSides = GetOpenSides(tangledPieces[nRow, nCol]);
                if (!neighborOpenSides[oppositeDir]) continue; // pieces don't actually connect

                visited[nRow, nCol] = true;
                queue.Enqueue(new Vector2Int(nRow, nCol));
            }
        }

        return false;
    }

    private void CompleteTangledTubePuzzle()
    {
        tubeRFixed = true;

        SpriteRenderer sr = BrokenTubeR.GetComponent<SpriteRenderer>();
        if (sr != null) sr.sprite = FixedTubeR;

        TangledTubeScreen.SetActive(false);
        TangledTubePieces.SetActive(false);
        SetMainSceneActive(true);
    }

    // ---------------------------------------------------------------
    // Handle puzzle (fixes Incomplete Table, unlocks Bird Stairs)
    // ---------------------------------------------------------------

    private void OpenHandlePuzzle()
    {
        SetMainSceneActive(false);
        HandlePuzzle.SetActive(true);
        HandleRot.SetActive(true);

        heldItem = HeldItem.None;
        heldTransform = null;

        Handle.transform.SetParent(HandleRot.transform);
        Handle.transform.localPosition = Vector3.zero;
    }

    public void OnHandleRotMouseDown()
    {
        trackingHandleRotation = true;
        accumulatedRotation = 0f;
        lastMouseDir = GetMouseDirectionFromHandleRot();
    }

    private void UpdateHandleRotation()
    {
        Vector3 currentDir = GetMouseDirectionFromHandleRot();
        float angleDelta = Vector3.SignedAngle(lastMouseDir, currentDir, Vector3.forward);

        // Clockwise drag reads as a negative signed angle in Unity's convention,
        // so flip the sign to make clockwise motion add positive progress.
        accumulatedRotation += -angleDelta;
        HandleRot.transform.Rotate(0f, 0f, angleDelta);
        lastMouseDir = currentDir;

        if (accumulatedRotation >= 360f)
        {
            trackingHandleRotation = false;
            SolveHandlePuzzle();
        }
    }

    private Vector3 GetMouseDirectionFromHandleRot()
    {
        Vector3 mouseWorldPos = GetMouseWorldPos();
        mouseWorldPos.z = HandleRot.transform.position.z;
        return (mouseWorldPos - HandleRot.transform.position).normalized;
    }

    private void SolveHandlePuzzle()
    {
        handlePuzzleSolved = true;

        SpriteRenderer tableSr = IncompleteTable.GetComponent<SpriteRenderer>();
        if (tableSr != null) tableSr.sprite = FixedHandleTable;

        Destroy(Handle);

        HandlePuzzle.SetActive(false);
        HandleRot.SetActive(false);
        SetMainSceneActive(true); // reenable previous saved state of the scene

        ReplaceDeadBirdWithStairs();
    }

    private void ReplaceDeadBirdWithStairs()
    {
        SpriteRenderer sr = DeadBird.GetComponent<SpriteRenderer>();
        if (sr != null) sr.sprite = BirdStairs;
        // DeadBird's existing collider2d is reused as the BirdStairs' clickable area;
        // OnBirdStairsClicked below only does anything once handlePuzzleSolved is true.
    }

    public void OnBirdStairsClicked()
    {
        if (!handlePuzzleSolved) return;
        CompleteRoom();
    }

    private void CompleteRoom()
    {
        Debug.Log("Room complete!");
        GameManager.Instance.MarkRoomComplete();
        // Hook up whatever should happen next: fade out, load next scene, play a cutscene, etc.
    }

    // ---------------------------------------------------------------
    // Cursor hover
    // ---------------------------------------------------------------

    // Called by ClickTarget.OnMouseEnter on any clickable collider.
    public void OnHoverEnter()
    {
        hoverCount++;
        UpdateCursor();
    }

    // Called by ClickTarget.OnMouseExit on any clickable collider.
    public void OnHoverExit()
    {
        hoverCount = Mathf.Max(0, hoverCount - 1);
        UpdateCursor();
    }

    private void UpdateCursor()
    {
        if (hoverCount > 0 && CursorHandPoint != null)
            Cursor.SetCursor(CursorHandPoint, cursorHotspot, CursorMode.Auto);
        else
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    // ---------------------------------------------------------------
    // Clue button (placeholder)
    // ---------------------------------------------------------------

    public void OnClueButtonClicked()
    {
        // Placeholder until the clue UI exists. Wire PuzzleClue into a
        // full-screen Image/CanvasGroup once that's built, e.g.:
        // clueImage.sprite = PuzzleClue; cluePanel.SetActive(true);
        Debug.Log("Show clue: " + (PuzzleClue != null ? PuzzleClue.name : "none set"));
    }

    // ---------------------------------------------------------------
    // Helpers
    // ---------------------------------------------------------------

    private void SetMainSceneActive(bool active)
    {
        foreach (GameObject obj in mainSceneObjects)
        {
            if (obj != null) obj.SetActive(active);
        }
    }
}