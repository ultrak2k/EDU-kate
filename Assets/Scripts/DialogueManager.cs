using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;



public enum Speaker
{
    Narrator,
    EDU,
    Kate
}

[System.Serializable]
public class DialogueLine
{
    [TextArea(2, 6)]
    public string text;

    public Speaker speaker;
}



public class DialogueManager : MonoBehaviour
{
    [SerializeField] private Parallax _parallax;
    [SerializeField] private GameObject _levelToSpawn;

    [Header("Dialogue Lines")]
    public DialogueLine[] lines;

    [Header("UI")]
    public TMP_Text speakerNameText;
    public TMP_Text dialogueBodyText;
    public GameObject dialogueBodyTextObject; // for enabling/disabling the text if needed
    public GameObject dialoguePanel;
    public GameObject NextSceneButton;
    public Button continueButton;

    [Header("Character Dialogue Sprites")]
    public GameObject EDUSprite;
    public GameObject KateSprite;
    public GameObject NarSprite;

    public SpriteRenderer KateHerself;
    public GameObject KatHologramEffect;

    public float typewriterSpeed = 0.03f;


    public bool EndKateDialogue = false;
    public GameObject Edu;
    public AudioClip DialogueAudioClip;
    public AudioClip KateHologramStart;
    public AudioClip KateHologramEnd;
    private bool firstKatSpeech = true;



    private int currentIndex = 0;
    private bool isTyping = false;
    private bool skipRequested = false;
    private Coroutine typewriterCoroutine;
    private void Start()
    {
        SetAllSpritesInactive();

        if(EndKateDialogue)
        {
            Edu.GetComponent<PlayerController>().enabled = false;
            Edu.GetComponent<PlayerInput>().enabled = false;
            Edu.GetComponentInChildren<Animator>().enabled = false;
            Edu.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero; // stop any movement
        }

        if (continueButton != null)
            continueButton.onClick.AddListener(OnAdvance);

        ShowLine(currentIndex);
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            AudioPlayer.Instance.PlayAudio(DialogueAudioClip);
            OnAdvance();
        }    

        /*if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            OnAdvance();*/ // removing left click cuz its clunky
    }



    private void OnAdvance()
    {
        if (isTyping)
        {
            skipRequested = true;
        }
        else
        {
            currentIndex++;
            if (currentIndex < lines.Length)
                ShowLine(currentIndex);
            else
                EndDialogue();
        }
    }

    private void ShowLine(int index)
    {
        DialogueLine line = lines[index];

        speakerNameText.text = line.speaker == Speaker.Narrator ? "" : line.speaker.ToString();

        ActivateSpriteFor(line.speaker);

        if (typewriterCoroutine != null)
            StopCoroutine(typewriterCoroutine);

        typewriterCoroutine = StartCoroutine(TypewriterRoutine(line.text));
    }

    private IEnumerator TypewriterRoutine(string fullText)
    {
        isTyping = true;
        skipRequested = false;
        dialogueBodyText.text = "";

        foreach (char c in fullText)
        {
            if (skipRequested)
            {
                dialogueBodyText.text = fullText;
                break;
            }

            dialogueBodyText.text += c;

            if (typewriterSpeed > 0f)
                yield return new WaitForSeconds(typewriterSpeed);
        }

        isTyping = false;
        skipRequested = false;
    }

 
    private void ActivateSpriteFor(Speaker speaker)
    {
        SetAllSpritesInactive();
        dialogueBodyTextObject.SetActive(true);
        switch (speaker)
        {
            case Speaker.EDU:
                if (EDUSprite != null)
                {
                    EDUSprite.SetActive(true);
                    if (firstKatSpeech)
                    {
                        firstKatSpeech = false;
                        AudioPlayer.Instance.PlayAudio(KateHologramStart);
                        KateHerself.enabled = true;
                        KatHologramEffect.SetActive(true);
                    }
                    
                }
                break;


            case Speaker.Kate:
                if (KateSprite != null)
                {
                    
                    KateSprite.SetActive(true);
                    if (firstKatSpeech)
                    {
                        firstKatSpeech = false;
                        AudioPlayer.Instance.PlayAudio(KateHologramStart);
                        KateHerself.enabled = true;
                        KatHologramEffect.SetActive(true);
                    }




                }
                break;
            case Speaker.Narrator:
                if (NarSprite != null)
                {
                    NarSprite.SetActive(true);
                }
                break;
        }
    }

    public void SetLevelToSpawn(GameObject prefabLevel)
    {
        _levelToSpawn = prefabLevel;
    }

    private void SetAllSpritesInactive()
    {
        if (EDUSprite != null) EDUSprite.SetActive(false);
        if (KateSprite != null) KateSprite.SetActive(false);
        if (NarSprite != null) NarSprite.SetActive(false);
    }
    
    private void EndDialogue()
    {

        if (EndKateDialogue)
        {
            Edu.GetComponent<PlayerController>().enabled = true;
            Edu.GetComponentInChildren<Animator>().enabled = true;
            Edu.GetComponent<PlayerInput>().enabled = true;

        }
        dialoguePanel.SetActive(false);
        dialogueBodyTextObject.SetActive(false);
        //NextSceneButton.SetActive(true);
        KateHerself.enabled = false;
        KatHologramEffect.SetActive(false);
        if(!EndKateDialogue)
        {

            AudioPlayer.Instance.PlayAudio(KateHologramEnd);
        }
        
        SetAllSpritesInactive();

        
        Instantiate(_levelToSpawn, _parallax.GetFurthestBackgroundPos(), Quaternion.identity);
        gameObject.SetActive(false);

    }
}