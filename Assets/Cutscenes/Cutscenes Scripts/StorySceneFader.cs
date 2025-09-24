using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StorySceneFader : MonoBehaviour
{
    public Image fadeImage;
    public float fadeDuration = 1.5f;

    void Start()
    {
        // start fully black → fade in
        if (fadeImage != null)
        {
            fadeImage.color = Color.black;
            StartCoroutine(FadeIn());
        }
    }

    IEnumerator FadeIn()
    {
        float t = 0;
        Color c = fadeImage.color;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(1, 0, t / fadeDuration);
            fadeImage.color = c;
            yield return null;
        }
        c.a = 0;
        fadeImage.color = c;
    }

    public void FadeOutAndLoad(string sceneName)
    {
        StartCoroutine(FadeOut(sceneName));
    }

    IEnumerator FadeOut(string sceneName)
    {
        float t = 0;
        Color c = fadeImage.color;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(0, 1, t / fadeDuration);
            fadeImage.color = c;
            yield return null;
        }
        c.a = 1;
        fadeImage.color = c;

        SceneManager.LoadScene(sceneName);
    }
}
