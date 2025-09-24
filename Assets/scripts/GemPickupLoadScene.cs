using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider2D))]
public class GemPickupLoadScene : MonoBehaviour
{
    [Header("Who can pick up")]
    public string playerTag = "Player";

    public enum LoadMode { NextInBuild, ByBuildIndex, BySceneName }

    [Header("Where to go")]
    public LoadMode loadMode = LoadMode.NextInBuild;

    [Tooltip("Used when Load Mode = ByBuildIndex. Set to a valid Build Settings index.")]
    public int targetBuildIndex = 1; // pick in Inspector

    [Tooltip("Used when Load Mode = BySceneName. Must match Build Settings scene name.")]
    public string targetSceneName = "";

    [Header("Optional")]
    public bool destroyOnPickup = false; // destroy this object after trigger

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

        // optional visuals cleanup
        if (destroyOnPickup) Destroy(gameObject);

        switch (loadMode)
        {
            case LoadMode.NextInBuild:
            {
                int i = SceneManager.GetActiveScene().buildIndex;
                SceneManager.LoadScene(i + 1);
                break;
            }
            case LoadMode.ByBuildIndex:
            {
                if (targetBuildIndex < 0 || targetBuildIndex >= SceneManager.sceneCountInBuildSettings)
                {
                    Debug.LogWarning($"GemPickupLoadScene: targetBuildIndex {targetBuildIndex} is out of range. " +
                                     "Check File → Build Settings.");
                    loading = false; // allow retry if you fix at runtime
                    return;
                }
                SceneManager.LoadScene(targetBuildIndex);
                break;
            }
            case LoadMode.BySceneName:
            {
                if (string.IsNullOrEmpty(targetSceneName))
                {
                    Debug.LogWarning("GemPickupLoadScene: targetSceneName is empty. Set a scene name.");
                    loading = false;
                    return;
                }
                SceneManager.LoadScene(targetSceneName);
                break;
            }
        }
    }
}
