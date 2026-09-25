using System;
using UnityEngine;

public class PlayerKeyStorage : MonoBehaviour
{
    public static PlayerKeyStorage Instance { get; private set; }

    private int keyCount;

    public int KeyCount => keyCount;

    public event Action<int> OnKeyCountChanged;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void CreateInstance()
    {
        if (Instance != null)
            return;

        GameObject storageObject =
            new GameObject("PlayerKeyStorage");

        storageObject.AddComponent<PlayerKeyStorage>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        keyCount = 0;
    }

    public void AddKey()
    {
        keyCount++;

        OnKeyCountChanged?.Invoke(keyCount);
    }

    public bool TryUseKey()
    {
        if (keyCount <= 0)
            return false;

        keyCount--;

        OnKeyCountChanged?.Invoke(keyCount);

        return true;
    }

    public void SetKeyCount(int amount)
    {
        keyCount = Mathf.Max(0, amount);

        OnKeyCountChanged?.Invoke(keyCount);
    }

    public void ResetProgress()
    {
        keyCount = 0;

        OnKeyCountChanged?.Invoke(keyCount);
    }
}