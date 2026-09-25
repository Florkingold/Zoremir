using UnityEngine;

public class ArtifactPanel : MonoBehaviour
{
    private const int SlotCount = 5;

    private ArtifactSlot[] slots;

    private void Awake()
    {
        FindSlots();
    }

    private void Start()
    {
        UpdateSlots();

        if (ArtifactInventory.Instance != null)
            ArtifactInventory.Instance.OnInventoryChanged += UpdateSlots;
    }

    private void OnDestroy()
    {
        if (ArtifactInventory.Instance != null)
            ArtifactInventory.Instance.OnInventoryChanged -= UpdateSlots;
    }

    private void FindSlots()
    {
        slots = GetComponentsInChildren<ArtifactSlot>(true);

        if (slots.Length != SlotCount)
        {
            Debug.LogError(
                $"ArtifactPanel requires exactly {SlotCount} ArtifactSlot objects. Found {slots.Length}.",
                this
            );

            return;
        }

        for (int i = 0; i < slots.Length; i++)
            slots[i].Initialize(i);
    }

    private void UpdateSlots()
    {
        if (slots == null)
            return;

        for (int i = 0; i < slots.Length; i++)
            slots[i].UpdateVisual();
    }

    public ArtifactSlot GetSlot(int index)
    {
        if (slots == null)
            return null;

        if (index < 0 || index >= slots.Length)
            return null;

        return slots[index];
    }
}