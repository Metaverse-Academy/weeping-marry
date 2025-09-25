using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OrbHUD : MonoBehaviour
{
    [Header("UI")]
    public Image orbIcon;      // your orb sprite here
    public TMP_Text  countText;    // shows the number (use TextMeshProUGUI if you prefer TMP)

    void OnEnable()
    {
        OrbCounter.OnChanged += UpdateCount;
        // initialize UI from current value if the counter already exists
        if (OrbCounter.Instance) UpdateCount(OrbCounter.Instance.Count);
    }

    void OnDisable()
    {
        OrbCounter.OnChanged -= UpdateCount;
    }

    void UpdateCount(int value)
    {
        if (countText) countText.text = value.ToString();
    }
}

