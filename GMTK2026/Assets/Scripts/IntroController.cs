using UnityEngine;
using UnityEngine.EventSystems;

public class IntroController :
    MonoBehaviour,
    IPointerClickHandler
{
    [SerializeField] private TypewriterText typewriter;

    [Header("Intro Root")]
    [SerializeField] private GameObject startObject;
    [SerializeField] private GameObject startScreen;
    [SerializeField] private GameObject introPanel;

    private bool introFinished;

    private void Start()
    {
        if (startObject == null)
            startObject = gameObject;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) ||
            Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            HandleContinueInput();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        HandleContinueInput();
    }

    private void HandleContinueInput()
    {
        if (typewriter == null)
            return;

        
        if (typewriter.IsTyping)
        {
            typewriter.SkipTyping();
            return;
        }
        
        
        FinishIntro();
    }

    private void FinishIntro()
    {
        if (introFinished)
            return;

        introFinished = true;

        if (startObject != null)
            startObject.SetActive(false);
    }
}