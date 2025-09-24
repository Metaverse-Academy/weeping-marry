using System.Collections;
using UnityEngine;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI dialogueText;
    public TextMeshProUGUI nameText;
    public GameObject spacePrompt;

    [Header("Dialogue Settings")]
    [TextArea(2, 5)] public string[] lines;   // Fill in Inspector
    public string[] speakers;                 // Fill in Inspector
    public float textSpeed = 0.03f;
    public string nextSceneName = "darkScene"; // Set in Inspector

    [Header("Audio Settings")]
    public AudioClip girlBlip;
    public AudioClip spiritBlip;

    private AudioSource audioSource;
    private int index;
    private bool isTyping;

    void Start()
    {
        dialogueText.text = "";
        nameText.text = "";
        if (spacePrompt != null) spacePrompt.SetActive(false);

        audioSource = GetComponent<AudioSource>();

        gameObject.SetActive(false); // hidden until Timeline triggers it
    }

    void Update()
    {
        if (gameObject.activeSelf && Input.GetKeyDown(KeyCode.Space))
        {
            if (isTyping)
            {
                // finish instantly
                StopAllCoroutines();
                dialogueText.text = lines[index];
                isTyping = false;
                if (spacePrompt != null) spacePrompt.SetActive(true);
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
        if (spacePrompt != null) spacePrompt.SetActive(false);

        if (speakers != null && speakers.Length > index)
            nameText.text = speakers[index];
        else
            nameText.text = "";

        foreach (char c in lines[index].ToCharArray())
        {
            dialogueText.text += c;

            // 🔊 Play audio depending on speaker
            if (audioSource != null)
            {
                if (speakers[index] == "Girl" && girlBlip != null)
                    audioSource.PlayOneShot(girlBlip);
                else if (speakers[index] == "Spirit" && spiritBlip != null)
                    audioSource.PlayOneShot(spiritBlip);
            }

            yield return new WaitForSeconds(textSpeed);
        }

        isTyping = false;
        if (spacePrompt != null) spacePrompt.SetActive(true);
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
        nameText.text = "";
        if (spacePrompt != null) spacePrompt.SetActive(false);

        StartCoroutine(WaitAndFadeOut());
    }

    IEnumerator WaitAndFadeOut()
    {
        yield return new WaitForSeconds(3f);

        StorySceneFader fader = FindFirstObjectByType<StorySceneFader>();
        if (fader != null)
        {
            fader.FadeOutAndLoad(nextSceneName);
        }

        // now safe to hide dialogue box
        gameObject.SetActive(false);
    }
}
