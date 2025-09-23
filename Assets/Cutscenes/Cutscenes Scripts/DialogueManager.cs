using System.Collections;
using UnityEngine;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public TextMeshProUGUI dialogueText;
    public string[] lines;         // set in Inspector per scene instance
    public float textSpeed = 0.03f;

    private int index;
    private bool isTyping;

    void Start()
    {
        dialogueText.text = "";
        gameObject.SetActive(false); // ensure off by default
    }

    void Update()
    {
        if (gameObject.activeSelf && Input.GetKeyDown(KeyCode.Space))
        {
            if (isTyping)
            {
                // finish current line immediately
                StopAllCoroutines();
                dialogueText.text = lines[index];
                isTyping = false;
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
        foreach (char c in lines[index].ToCharArray())
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(textSpeed);
        }
        isTyping = false;
    }

    void NextLine()
    {
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
        dialogueText.text = "";
        gameObject.SetActive(false);
        // Timeline or SceneLoader signal will handle next steps (fade / load).
    }
}
