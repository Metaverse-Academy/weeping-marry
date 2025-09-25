using UnityEngine;
using System;

public class OrbCounter : MonoBehaviour
{
    public static OrbCounter Instance { get; private set; }
    public static event Action<int> OnChanged;

    [SerializeField] private int count = 0;     // current collected
    public  int Count => count;

    [Header("Persist Across Scenes?")]
    public bool dontDestroyOnLoad = true;

    void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        if (dontDestroyOnLoad) DontDestroyOnLoad(gameObject);
        // notify UI on start so it displays 0
        OnChanged?.Invoke(count);
    }

    public static void Add(int amount)
    {
        if (!Instance) return;
        if (amount == 0) return;

        Instance.count = Mathf.Max(0, Instance.count + amount);
        OnChanged?.Invoke(Instance.count);
    }

    public static void ResetToZero()
    {
        if (!Instance) return;
        Instance.count = 0;
        OnChanged?.Invoke(Instance.count);
    }
}

