using System;
using UnityEngine;

public class ArtifactInventory : MonoBehaviour
{
    public const int MaxSlots = 5;
    public const int InitialUnlockedSlots = 3;

    private const string UnlockedSlotsKey = "ArtifactInventory_UnlockedSlots";
    private const string DataVersionKey = "ArtifactInventory_DataVersion";
    private const int CurrentDataVersion = 3;

    public static ArtifactInventory Instance { get; private set; }

    public int UnlockedSlots { get; private set; }

    public event Action OnInventoryChanged;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void CreateInstance()
    {
        if (Instance != null)
            return;

        GameObject inventoryObject =
            new GameObject("ArtifactInventory");

        inventoryObject.AddComponent<ArtifactInventory>();
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

        LoadProgress();
    }

    public bool IsSlotUnlocked(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= MaxSlots)
            return false;

        return slotIndex < UnlockedSlots;
    }

    public bool UnlockNextSlot()
    {
        if (UnlockedSlots >= MaxSlots)
            return false;

        UnlockedSlots++;

        SaveProgress();
        OnInventoryChanged?.Invoke();

        return true;
    }

    public void ResetProgress()
    {
        UnlockedSlots = InitialUnlockedSlots;

        SaveProgress();
        OnInventoryChanged?.Invoke();
    }

    private void LoadProgress()
    {
        int dataVersion =
            PlayerPrefs.GetInt(
                DataVersionKey,
                0
            );

        if (dataVersion < CurrentDataVersion)
        {
            UnlockedSlots = InitialUnlockedSlots;

            SaveProgress();

            return;
        }

        UnlockedSlots = PlayerPrefs.GetInt(
            UnlockedSlotsKey,
            InitialUnlockedSlots
        );

        UnlockedSlots = Mathf.Clamp(
            UnlockedSlots,
            InitialUnlockedSlots,
            MaxSlots
        );
    }

    private void SaveProgress()
    {
        PlayerPrefs.SetInt(
            UnlockedSlotsKey,
            UnlockedSlots
        );

        PlayerPrefs.SetInt(
            DataVersionKey,
            CurrentDataVersion
        );

        PlayerPrefs.Save();
    }
}