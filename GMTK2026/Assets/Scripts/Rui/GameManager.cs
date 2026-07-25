using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public enum GameStates { Menu, RoomA1, RoomA2, RoomB1, RoomB2, RoomC }
    public GameStates gameState;
    [Header("UI Objects")]
        [SerializeField] private Button CharA;
        [SerializeField] private Button CharB;
        [SerializeField] private TMP_Text Timer;

    [Header("Timers")]
        private float totalTimeA;
        private float totalTimeB;
        private float timer = 0f; // this is used to count up 
        private float countdown; // this is used to count down 
        private const float tieThreshold = 10f; // determines how close in time both characters are for ending conditions
        private float previousRoomTime = 0f; 
    [Header("Display Character")]
        private bool showA = false;
        private bool showB = false;
    [Header("Test/Debug UI")]
        [SerializeField] private Button TestCompleteButton; // "TestComplete" button in rooms A1, A2, B1, B2
        [SerializeField] private TMP_Text ATimeText;        // "ATime" text in Room C
        [SerializeField] private TMP_Text BTimeText;        // "BTime" text in Room C
    [Header("Rooms")]
        private bool AisFirstRoom = false;
        private bool BisFirstRoom = false;
        public bool roomComplete = false;
        // Position within the 4-room sequence for the current playthrough (1-4)
        // Odd = fresh "count up" room (saves its own total directly)
        // Even = derived "countdown" room (seeded from the other character's total)
        private int currentRoomPosition = 0;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); 
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Timer UI is a fresh object in every room scene (named "Time"), it does not
        // persist like GameManager does, so we have to re-find and re-hook it each load.
        GameObject timeObj = GameObject.Find("Time");
        Timer = timeObj != null ? timeObj.GetComponent<TMP_Text>() : null;

        // Title screen buttons also live in their own scene; re-wire them if we're back there.
        if (gameState == GameStates.Menu)
        {
            GameObject charAObj = GameObject.Find("Character A");
            GameObject charBObj = GameObject.Find("Character B");
            CharA = charAObj != null ? charAObj.GetComponent<Button>() : null;
            CharB = charBObj != null ? charBObj.GetComponent<Button>() : null;
            if (CharA != null) CharA.onClick.AddListener(StartRouteA);
            if (CharB != null) CharB.onClick.AddListener(StartRouteB);
        }
        // Rooms A1/A2/B1/B2 each have a "TestComplete" button standing in for real completion logic
        else if (gameState == GameStates.RoomA1 || gameState == GameStates.RoomA2 ||
                 gameState == GameStates.RoomB1 || gameState == GameStates.RoomB2)
        {
            GameObject testBtnObj = GameObject.Find("TestComplete");
            TestCompleteButton = testBtnObj != null ? testBtnObj.GetComponent<Button>() : null;
            if (TestCompleteButton != null)
            {
                TestCompleteButton.onClick.RemoveAllListeners();
                TestCompleteButton.onClick.AddListener(MarkRoomComplete);
            }
        }
        // Room C displays the final backend totals for verification
        else if (gameState == GameStates.RoomC)
        {
            GameObject aTimeObj = GameObject.Find("ATime");
            GameObject bTimeObj = GameObject.Find("BTime");
            ATimeText = aTimeObj != null ? aTimeObj.GetComponent<TMP_Text>() : null;
            BTimeText = bTimeObj != null ? bTimeObj.GetComponent<TMP_Text>() : null;
            if (ATimeText != null) ATimeText.text = totalTimeA.ToString("F1");
            if (BTimeText != null) BTimeText.text = totalTimeB.ToString("F1");
        }
    }
    private void MarkRoomComplete()
    {
        roomComplete = true;
    }

    private void Start()
    {
        if (gameState == GameStates.Menu)
        {
            if (CharA != null) CharA.onClick.AddListener(StartRouteA);
            if (CharB != null) CharB.onClick.AddListener(StartRouteB);
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
            timer += Time.deltaTime;
            UpdateTimerDisplay();
        }
    }
    private void UpdateTimerDisplay() {
        if (Timer == null) return;
        bool isFreshRoom = (currentRoomPosition % 2 == 1);
        
        if (isFreshRoom) {
            // Counting up from zero
            Timer.text = timer.ToString("F1");
        }
        else {
            // Counting down from the other character's most recent total
            /*float seed = (gameState == GameStates.RoomA1 || gameState == GameStates.RoomA2)
                ? totalTimeB
                : totalTimeA;
            countdown = seed - timer;
            Timer.text = countdown > 0f
                ? countdown.ToString("F1")
                : "+" + (-countdown).ToString("F1");*/ // in carry/overtime
            countdown = previousRoomTime - timer;
            Timer.text = countdown > 0f
                ? countdown.ToString("F1")
                : "+" + (-countdown).ToString("F1");
        }
    }
    private void StartRouteA() {
        AisFirstRoom = true;
        totalTimeA = 0f;
        totalTimeB = 0f;
        LoadNewRoom(GameStates.RoomA1, "Room A1", 1);
    }
    private void StartRouteB() {
        BisFirstRoom = true;
        totalTimeA = 0f;
        totalTimeB = 0f;
        LoadNewRoom(GameStates.RoomB1, "Room B1", 1);
    }
    private void ManageScenes() {
        // Route A: A1(1,fresh) -> B1(2,derived) -> B2(3,fresh) -> A2(4,derived) -> C
        // Route B: B1(1,fresh) -> A1(2,derived) -> A2(3,fresh) -> B2(4,derived) -> C
        switch (gameState)
        {
            case GameStates.RoomA1:
                if (AisFirstRoom)
                    LoadNewRoom(GameStates.RoomB1, "Room B1", 2);
                else
                    LoadNewRoom(GameStates.RoomA2, "Room A2", 3);
                break;
            case GameStates.RoomB1:
                if (AisFirstRoom)
                    LoadNewRoom(GameStates.RoomB2, "Room B2", 3);
                else
                    LoadNewRoom(GameStates.RoomA1, "Room A1", 2);
                break;
            case GameStates.RoomB2:
                if (AisFirstRoom)
                    LoadNewRoom(GameStates.RoomA2, "Room A2", 4);
                else
                    LoadNewRoom(GameStates.RoomC, "Room C", 0);
                break;
            case GameStates.RoomA2:
                if (AisFirstRoom)
                    LoadNewRoom(GameStates.RoomC, "Room C", 0);
                else
                    LoadNewRoom(GameStates.RoomB2, "Room B2", 4);
                break;
            case GameStates.RoomC:
                resolveC();
                break;
        }
    }
    private void LoadNewRoom(GameStates newState, string sceneName, int position) {
        // Save the time spent in the room we're leaving (if any) before switching state.
        // This is a true accumulating sum in the backend (A1+A2, B1+B2) regardless of
        // whether the room was displayed as a fresh count-up or a derived countdown.
        if (currentRoomPosition > 0)
        {
            previousRoomTime = timer;
            SaveRoomTime(gameState);
        }
        timer = 0f;
        countdown = 0f;
        currentRoomPosition = position;
        gameState = newState;
        SceneManager.LoadScene(sceneName);
    }
    private void SaveRoomTime(GameStates room) {
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
        if (timeDifference <= tieThreshold) {
            showA = true;
            showB = true;
            Debug.Log("Both characters show up!");
        }
        else if (totalTimeA < totalTimeB) {
            showA = true;
            showB = false;
            Debug.Log("Character A is faster!");
        }
        else {
            showA = false;
            showB = true;
            Debug.Log("Character B is faster!");
        }
    }
}