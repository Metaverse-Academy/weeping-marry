using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider2D))]
public class GemPickupLoadScene : MonoBehaviour
{
    [Header("Who can pick up")]
    public string playerTag = "Player";

    [Header("Where to go")]
    public bool loadNextInBuild = true;       // if true → next scene by build index
    public string sceneNameIfNotNext = "";    // otherwise load this exact scene name

    bool loading; // prevents double-trigger

    void Reset()
    {
        // Make sure collider acts as a trigger
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (loading) return;
        if (!other.CompareTag(playerTag)) return;

        loading = true;

        // (Optional) remove the gem visuals
        // Destroy(gameObject); // uncomment if the whole object is just the gem

        if (loadNextInBuild)
        {
            int i = SceneManager.GetActiveScene().buildIndex;
            SceneManager.LoadScene(i + 1);
        }
        else
        {
            if (!string.IsNullOrEmpty(sceneNameIfNotNext))
                SceneManager.LoadScene(sceneNameIfNotNext);
            else
                Debug.LogWarning("GemPickupLoadScene: No scene name set and loadNextInBuild is false.");
        }
    }
}

