using System.Collections;
using TMPro;
using UnityEngine;

public class TypewriterText : MonoBehaviour
{
    [Header("Text")]
    [SerializeField] private TMP_Text textBox;

    [TextArea(3, 12)]
    [SerializeField] private string fullText;

    [Header("Typing Speed")]
    [SerializeField] private float characterDelay = 0.04f;

    [Header("UI")]
    [SerializeField] private GameObject continueHint;

    private Coroutine typingCoroutine;

    public bool IsTyping { get; private set; }

    private void Awake()
    {
        if (textBox == null)
            textBox = GetComponent<TMP_Text>();
    }

    private void OnEnable()
    {
        ResetAndPlay();
    }

    private void OnDisable()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        IsTyping = false;
    }

    public void ResetAndPlay()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        if (continueHint != null)
            continueHint.SetActive(false);

        textBox.text = fullText;
        textBox.ForceMeshUpdate();

        textBox.maxVisibleCharacters = 0;

        typingCoroutine = StartCoroutine(
            TypeText()
        );
    }

    private IEnumerator TypeText()
    {
        IsTyping = true;

        textBox.ForceMeshUpdate();

        TMP_TextInfo textInfo =
            textBox.textInfo;

        int totalCharacters =
            textInfo.characterCount;

        for (int i = 0; i < totalCharacters; i++)
        {
            textBox.maxVisibleCharacters =
                i + 1;

            char currentCharacter =
                textInfo.characterInfo[i].character;

            yield return new WaitForSeconds(
                GetCharacterDelay(
                    currentCharacter
                )
            );
        }

        textBox.maxVisibleCharacters =
            totalCharacters;

        IsTyping = false;
        typingCoroutine = null;

        if (continueHint != null)
            continueHint.SetActive(true);
    }

    private float GetCharacterDelay(
        char character
    )
    {
        switch (character)
        {
            case '.':
            case '?':
            case '!':
                return characterDelay * 6f;

            case ',':
            case ';':
            case ':':
                return characterDelay * 3f;

            default:
                return characterDelay;
        }
    }

    public void SkipTyping()
    {
        if (!IsTyping)
            return;

        if (typingCoroutine != null)
        {
            StopCoroutine(
                typingCoroutine
            );

            typingCoroutine = null;
        }

        textBox.ForceMeshUpdate();

        textBox.maxVisibleCharacters =
            textBox.textInfo.characterCount;

        IsTyping = false;

        if (continueHint != null)
            continueHint.SetActive(true);
    }
}