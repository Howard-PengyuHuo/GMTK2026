using UnityEngine;

// Attach one of these to every interactable GameObject that has a Collider2D:
// Tape, Handle, BrokenTubeL, IncompleteTable, TangledTubeButton, ClueButton,
// each tangled tube piece, HandleRot, and DeadBird/BirdStairs.
// Set the matching "type" in the Inspector and drag in the PuzzleManager.
[RequireComponent(typeof(Collider2D))]
public class ClickTarget : MonoBehaviour
{
    public enum TargetType
    {
        Tape,
        Handle,
        TangledTubeButton,
        TangledTubePiece,
        ClueButton,
        HandleRot,
        BirdStairs
    }

    [SerializeField] private PuzzleManager puzzleManager;
    [SerializeField] private TargetType type;

    private void OnMouseDown()
    {
        switch (type)
        {
            case TargetType.Tape:
                puzzleManager.OnTapeClicked();
                break;
            case TargetType.Handle:
                puzzleManager.OnHandleClicked();
                break;
            case TargetType.TangledTubeButton:
                puzzleManager.OnTangledTubeButtonClicked();
                break;
            case TargetType.TangledTubePiece:
                puzzleManager.OnTangledPieceClicked(transform);
                break;
            case TargetType.ClueButton:
                puzzleManager.OnClueButtonClicked();
                break;
            case TargetType.HandleRot:
                puzzleManager.OnHandleRotMouseDown();
                break;
            case TargetType.BirdStairs:
                puzzleManager.OnBirdStairsClicked();
                break;
        }
    }

    private void OnMouseEnter()
    {
        puzzleManager.OnHoverEnter();
    }

    private void OnMouseExit()
    {
        puzzleManager.OnHoverExit();
    }
}