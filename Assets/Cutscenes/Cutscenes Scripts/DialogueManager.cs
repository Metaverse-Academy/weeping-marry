using System.Collections;
using UnityEngine;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI dialogueText;   // Main dialogue text
    public TextMeshProUGUI speakerText;    // Character name
    public GameObject spaceIcon;           // [ Space ] hint UI

    [Header("Dialogue Settings")]
    [TextArea(2, 6)]
    public string[] lines;                 // Dialogue lines
    public string[] speakers;              // Speaker names (same length as lines)
    public float textSpeed = 0.03f;        // Typing speed

    private int index;
    private bool isTyping;
    private Coroutine blinkCoroutine;

    void Start()
    {
        dialogueText.text = "";
        speakerText.text = "";
        spaceIcon.SetActive(false);
        gameObject.SetActive(false);
    }

    void Update()
    {
        if (gameObject.activeSelf && Input.GetKeyDown(KeyCode.Space))
        {
            if (isTyping)
            {
                // Skip typing effect and finish line instantly
                StopAllCoroutines();
                dialogueText.text = lines[index];
                isTyping = false;

                // Show [Space] icon immediately
                blinkCoroutine = StartCoroutine(BlinkIcon());
            }
            else
            {
                NextLine();
            }
        }
    }

    public void StartDialogue()
    {
        if (lines == null || lines.Length == 0) return;
        index = 0;
        gameObject.SetActive(true);
        StartCoroutine(TypeLine());
    }

    IEnumerator TypeLine()
    {
        isTyping = true;
        dialogueText.text = "";
        spaceIcon.SetActive(false);

        // Set speaker name if available
        if (speakers != null && index < speakers.Length)
        {
            speakerText.text = speakers[index];
        }
        else
        {
            speakerText.text = "";
        }

        foreach (char c in lines[index].ToCharArray())
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(textSpeed);
        }

        isTyping = false;
        blinkCoroutine = StartCoroutine(BlinkIcon());
    }

    IEnumerator BlinkIcon()
    {
        spaceIcon.SetActive(true);
        while (true)
        {
            spaceIcon.SetActive(!spaceIcon.activeSelf);
            yield return new WaitForSeconds(0.5f);
        }
    }

    void NextLine()
    {
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            spaceIcon.SetActive(false);
        }

        if (index < lines.Length - 1)
        {
            index++;
            StartCoroutine(TypeLine());
        }
        else
        {
            EndDialogue();
        }
    }

    void EndDialogue()
    {
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
        }

        spaceIcon.SetActive(false);
        dialogueText.text = "";
        speakerText.text = "";
        gameObject.SetActive(false);

        // Optionally: notify Timeline/SceneLoader here
    }
}
