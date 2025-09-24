using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Button))]
public class ButtonLoadSceneByIndex : MonoBehaviour
{
    [Header("Scene to load (Build Settings index)")]
    [SerializeField] private int targetBuildIndex = 1;

    [Header("Auto-wire OnClick")]
    [SerializeField] private bool autoWireOnClick = true;

    Button btn;

    void Awake()
    {
        btn = GetComponent<Button>();

        if (autoWireOnClick)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(LoadTargetScene);
        }
    }

    // You can also hook this manually in the Button's OnClick, if you turn off autoWireOnClick
    public void LoadTargetScene()
    {
        int count = SceneManager.sceneCountInBuildSettings;
        if (targetBuildIndex < 0 || targetBuildIndex >= count)
        {
            Debug.LogWarning(
                $"ButtonLoadSceneByIndex: targetBuildIndex {targetBuildIndex} is out of range (0..{count - 1}). " +
                "Add your scenes to File → Build Settings and set a valid index.");
            return;
        }

        SceneManager.LoadScene(targetBuildIndex);
    }
}
