using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {
    public static GameManager Instance { get; private set; }
    public enum GameStates { Menu, RoomA1, RoomA2, RoomB1, RoomB2, RoomC }
    public GameStates gameState;

    private static readonly Dictionary<GameStates, string> SceneNames = new Dictionary<GameStates, string>
    {
        { GameStates.Menu,   "Menu" },
        { GameStates.RoomA1, "Room A1" },
        { GameStates.RoomA2, "Room A2" },
        { GameStates.RoomB1, "Room B1" },
        { GameStates.RoomB2, "Room B2" },
        { GameStates.RoomC,  "Room C" },
    };

    private const string TimeObjName = "Time";
    private const string NeedleObjName = "Clock Needle";
    private const string CharAObjName = "Character A";
    private const string CharBObjName = "Character B";
    private const string TestCompleteObjName = "TestComplete";
    private const string ATimeObjName = "ATime";
    private const string BTimeObjName = "BTime";

    [Header("UI Objects")]
        [SerializeField] private Button CharA;
        [SerializeField] private Button CharB;
        [SerializeField] private TMP_Text Timer;

    [Header("Timers")]
        private float totalTimeA;
        private float totalTimeB;
        private float timer = 0f; 
        private float countdown; 
        private const float tieThreshold = 10f; 
        private float previousRoomTime = 0f;
        private float lastDisplayedValue = float.NaN; 

    [Header("Clock Needle")]
        [SerializeField] private Transform clockNeedle;      
        [SerializeField] private float minutesPerRotation = 5f; 
        private float currentNeedleAngle = 0f;
        private float previousNeedleAngle = 0f;
        private float degreesPerSecond; 

    [Header("Display Character")]
        private bool showA = false;
        private bool showB = false;

    [Header("Test/Debug UI")]
        [SerializeField] private Button TestCompleteButton; 
        [SerializeField] private TMP_Text ATimeText;       
        [SerializeField] private TMP_Text BTimeText;        

    [Header("Rooms")]
        private bool AisFirstRoom = false;
        private bool BisFirstRoom = false;
        public bool roomComplete = false;
            // Position within the 4-room sequence for the current playthrough (1-4)
            // Odd = fresh "count up" room (saves its own total directly)
            // Even = derived "countdown" room (seeded from the other character's total)
        private int currentRoomPosition = 0;


    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
        RecalculateNeedleSpeed();
    }

    private void OnDestroy() {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnValidate() {
        RecalculateNeedleSpeed();
    }

    private void RecalculateNeedleSpeed() {
        float minutes = Mathf.Max(minutesPerRotation, 0.0001f); // guard against div-by-zero from the Inspector
        degreesPerSecond = 360f / (minutes * 60f);
    }

    private static bool IsFreshPosition(int position) => position % 2 == 1;

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        GameObject timeObj = GameObject.Find(TimeObjName);
        Timer = timeObj != null ? timeObj.GetComponent<TMP_Text>() : null;
        lastDisplayedValue = float.NaN; // force a fresh text update on first frame of the new room

        GameObject needleObj = GameObject.Find(NeedleObjName);
        clockNeedle = needleObj != null ? needleObj.transform : null;
        if (clockNeedle != null) {
            clockNeedle.localRotation = Quaternion.Euler(0f, 0f, currentNeedleAngle);
        }

        if (gameState == GameStates.Menu) {
            WireMenuButtons();
        }
        else if (gameState == GameStates.RoomA1 || gameState == GameStates.RoomA2 ||
                 gameState == GameStates.RoomB1 || gameState == GameStates.RoomB2)
        {
            GameObject testBtnObj = GameObject.Find(TestCompleteObjName);
            TestCompleteButton = testBtnObj != null ? testBtnObj.GetComponent<Button>() : null;
            if (TestCompleteButton != null) {
                TestCompleteButton.onClick.RemoveAllListeners();
                TestCompleteButton.onClick.AddListener(MarkRoomComplete);
            }
        }
        else if (gameState == GameStates.RoomC)
        {
            GameObject aTimeObj = GameObject.Find(ATimeObjName);
            GameObject bTimeObj = GameObject.Find(BTimeObjName);
            ATimeText = aTimeObj != null ? aTimeObj.GetComponent<TMP_Text>() : null;
            BTimeText = bTimeObj != null ? bTimeObj.GetComponent<TMP_Text>() : null;
            if (ATimeText != null) ATimeText.text = totalTimeA.ToString("F1");
            if (BTimeText != null) BTimeText.text = totalTimeB.ToString("F1");
        }
    }

    private void WireMenuButtons() {
        GameObject charAObj = GameObject.Find(CharAObjName);
        GameObject charBObj = GameObject.Find(CharBObjName);
        CharA = charAObj != null ? charAObj.GetComponent<Button>() : null;
        CharB = charBObj != null ? charBObj.GetComponent<Button>() : null;

        if (CharA != null)
        {
            CharA.onClick.RemoveAllListeners();
            CharA.onClick.AddListener(StartRouteA);
        }
        if (CharB != null)
        {
            CharB.onClick.RemoveAllListeners();
            CharB.onClick.AddListener(StartRouteB);
        }
    }

    private void MarkRoomComplete() {
        roomComplete = true;
    }

    private void Start() {
        if (gameState == GameStates.Menu)
        {
            WireMenuButtons();
        }
    }

    void Update() {
        if (roomComplete)
        {
            roomComplete = false; // reset to prevent infinite loop
            ManageScenes();
        }
        if (gameState != GameStates.Menu && gameState != GameStates.RoomC)
        {
            float dt = Time.deltaTime; // read once per frame, reused below
            timer += dt;
            UpdateTimerDisplay(dt);
        }
    }

    private void UpdateTimerDisplay(float dt) {
        bool isFreshRoom = IsFreshPosition(currentRoomPosition);

        if (Timer != null)
        {
            float displayValue;
            bool isOvertime = false;

            if (isFreshRoom)
            {
                displayValue = timer;
            }
            else
            {
                countdown = previousRoomTime - timer;
                isOvertime = countdown <= 0f;
                displayValue = isOvertime ? -countdown : countdown;
            }

            float rounded = Mathf.Round(displayValue * 10f) * 0.1f;
            if (!Mathf.Approximately(rounded, lastDisplayedValue))
            {
                lastDisplayedValue = rounded;
                Timer.text = isOvertime ? "+" + rounded.ToString("F1") : rounded.ToString("F1");
            }
        }

        RotateClockNeedle(isFreshRoom, dt);
    }

    private void RotateClockNeedle(bool isFreshRoom, float dt)
    {
        if (clockNeedle == null) return;

        float direction = isFreshRoom ? -1f : 1f; // clockwise while counting up, counterclockwise while counting down
        currentNeedleAngle += direction * degreesPerSecond * dt;
        clockNeedle.localRotation = Quaternion.Euler(0f, 0f, currentNeedleAngle);
    }

    private void StartRouteA()
    {
        AisFirstRoom = true;
        totalTimeA = 0f;
        totalTimeB = 0f;
        LoadNewRoom(GameStates.RoomA1, 1);
    }
    private void StartRouteB()
    {
        BisFirstRoom = true;
        totalTimeA = 0f;
        totalTimeB = 0f;
        LoadNewRoom(GameStates.RoomB1, 1);
    }

    private void ManageScenes()
    {
        // Route A: A1(1,fresh) -> B1(2,derived) -> B2(3,fresh) -> A2(4,derived) -> C
        // Route B: B1(1,fresh) -> A1(2,derived) -> A2(3,fresh) -> B2(4,derived) -> C
        switch (gameState)
        {
            case GameStates.RoomA1:
                if (AisFirstRoom)
                    LoadNewRoom(GameStates.RoomB1, 2);
                else
                    LoadNewRoom(GameStates.RoomA2, 3);
                break;
            case GameStates.RoomB1:
                if (AisFirstRoom)
                    LoadNewRoom(GameStates.RoomB2, 3);
                else
                    LoadNewRoom(GameStates.RoomA1, 2);
                break;
            case GameStates.RoomB2:
                if (AisFirstRoom)
                    LoadNewRoom(GameStates.RoomA2, 4);
                else
                    LoadNewRoom(GameStates.RoomC, 0);
                break;
            case GameStates.RoomA2:
                if (AisFirstRoom)
                    LoadNewRoom(GameStates.RoomC, 0);
                else
                    LoadNewRoom(GameStates.RoomB2, 4);
                break;
            case GameStates.RoomC:
                resolveC();
                break;
        }
    }

    private void LoadNewRoom(GameStates newState, int position)
    {
        if (currentRoomPosition > 0)
        {
            previousRoomTime = timer;
            SaveRoomTime(gameState);

            if (IsFreshPosition(currentRoomPosition))
            {
                previousNeedleAngle = currentNeedleAngle;
            }
        }

        timer = 0f;
        countdown = 0f;
        currentRoomPosition = position;
        gameState = newState;

        // Fresh rooms always start the needle at 0deg; derived rooms pick up from the last fresh room's ending angle
        currentNeedleAngle = IsFreshPosition(position) ? 0f : previousNeedleAngle;

        if (!SceneNames.TryGetValue(newState, out string sceneName))
        {
            Debug.LogError($"No scene name registered for state {newState}");
            return;
        }
        SceneManager.LoadScene(sceneName);
    }

    private void SaveRoomTime(GameStates room)
    {
        switch (room)
        {
            case GameStates.RoomA1:
            case GameStates.RoomA2:
                totalTimeA += timer;
                break;
            case GameStates.RoomB1:
            case GameStates.RoomB2:
                totalTimeB += timer;
                break;
        }
    }

    private void resolveC()
    {
        float timeDifference = Mathf.Abs(totalTimeA - totalTimeB);
        if (timeDifference <= tieThreshold)
        {
            showA = true;
            showB = true;
            Debug.Log("Both characters show up!");
        }
        else if (totalTimeA < totalTimeB)
        {
            showA = true;
            showB = false;
            Debug.Log("Character A is faster!");
        }
        else
        {
            showA = false;
            showB = true;
            Debug.Log("Character B is faster!");
        }
    }
}