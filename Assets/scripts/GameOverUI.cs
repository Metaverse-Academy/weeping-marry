using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GameOverUI : MonoBehaviour
{
    [Header("Optional (auto-found if left empty)")]
    [SerializeField] private GameObject gameOverPanel; // parent object with both buttons
    [SerializeField] private Button restartButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Healthmyversion playerHealth;

    [Header("Build Indexes")]
    [Tooltip("0 = your game scene (e.g., 'Raneem fixed scene')")]
    [SerializeField] private int gameSceneIndex = 0;
    [Tooltip("1 = your main menu scene (e.g., 'Main Menu')")]
    [SerializeField] private int mainMenuSceneIndex = 1;

    private void Awake()
    {
        EnsureEventSystem();
    }

    private void Start()
    {
        // Find panel if not set
        if (gameOverPanel == null)
            gameOverPanel = FindGameOverPanel();

        // Hide panel initially
        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        // Auto-find the player's Health if not assigned
        if (playerHealth == null) playerHealth = FindFirstObjectByType<Healthmyversion>();
        if (playerHealth != null) playerHealth.OnDied += ShowGameOver;

        // Auto-wire buttons under the panel
        AutoWireButtons();
    }

    private void OnDestroy()
    {
        if (playerHealth != null) playerHealth.OnDied -= ShowGameOver;
        if (restartButton != null) restartButton.onClick.RemoveListener(RestartGame);
        if (quitButton != null) quitButton.onClick.RemoveListener(QuitToMainMenu);
    }

    private void ShowGameOver()
    {
        Debug.Log("💀 Player died → showing Game Over panel");
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        Time.timeScale = 0f; // pause the game so UI is readable
    }

    // Reload the game scene
    public void RestartGame()
    {
        Debug.Log("🔄 Restart pressed → reloading game scene (index 0)");
        Time.timeScale = 1f;
        SceneManager.LoadScene(gameSceneIndex);
    }

    // Go back to Main Menu
    public void QuitToMainMenu()
    {
        Debug.Log("↩ Quit pressed → loading Main Menu (index 1)");
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneIndex);
    }

    // -------- helpers --------

    void AutoWireButtons()
    {
        if (gameOverPanel == null)
        {
            Debug.LogError("GameOverUI: No GameOverPanel found/assigned. Make sure you have a panel object under Canvas that contains your Restart and Quit buttons.");
            return;
        }

        if (restartButton == null) restartButton = FindButtonInChildren(gameOverPanel, "restart");
        if (quitButton == null)    quitButton    = FindButtonInChildren(gameOverPanel, "quit");

        if (restartButton != null)
        {
            restartButton.onClick.RemoveAllListeners();
            restartButton.onClick.AddListener(RestartGame);
            Debug.Log("✅ Restart button wired.");
        }
        else
        {
            Debug.LogError("❌ Could not find a button named with 'restart' under GameOverPanel.");
        }

        if (quitButton != null)
        {
            quitButton.onClick.RemoveAllListeners();
            quitButton.onClick.AddListener(QuitToMainMenu);
            Debug.Log("✅ Quit button wired.");
        }
        else
        {
            Debug.LogError("❌ Could not find a button named with 'quit' under GameOverPanel.");
        }
    }

    Button FindButtonInChildren(GameObject root, string nameContains)
    {
        if (!root) return null;
        var buttons = root.GetComponentsInChildren<Button>(true);
        foreach (var b in buttons)
        {
            if (b.name.ToLower().Contains(nameContains.ToLower()))
                return b;
        }
        return null;
    }

    GameObject FindGameOverPanel()
    {
        // Try to find a child under any Canvas with "gameover" in the name
        var canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        foreach (var c in canvases)
        {
            foreach (Transform child in c.transform)
            {
                if (child.name.ToLower().Contains("gameover"))
                    return child.gameObject;
            }
        }
        return null;
    }

    void EnsureEventSystem()
    {
        if (FindFirstObjectByType<EventSystem>() == null)
        {
            var go = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            Debug.Log("Created EventSystem (none was found).");
        }
    }
}
