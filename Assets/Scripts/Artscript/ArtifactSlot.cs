using UnityEngine;

public class ArtifactSlot : MonoBehaviour
{
    [SerializeField] private GameObject blockedVisual;

    private Artifact artifact;
    private int slotIndex;

    public int SlotIndex => slotIndex;

    public bool IsUnlocked =>
        ArtifactInventory.Instance != null &&
        ArtifactInventory.Instance.IsSlotUnlocked(slotIndex);

    public bool IsBlocked => !IsUnlocked;

    public bool IsOccupied => artifact != null;

    public Artifact Artifact => artifact;

    public void Initialize(int index)
    {
        slotIndex = index;
        UpdateVisual();
    }

    public bool CanAcceptArtifact()
    {
        return IsUnlocked && artifact == null;
    }

    public bool SetArtifact(Artifact newArtifact)
    {
        if (newArtifact == null)
            return false;

        if (!CanAcceptArtifact())
            return false;

        RectTransform artifactTransform =
            newArtifact.transform as RectTransform;

        if (artifactTransform == null)
            return false;

        artifact = newArtifact;

        artifactTransform.SetParent(
            transform,
            false
        );

        artifactTransform.anchorMin =
            new Vector2(0.5f, 0.5f);

        artifactTransform.anchorMax =
            new Vector2(0.5f, 0.5f);

        artifactTransform.pivot =
            new Vector2(0.5f, 0.5f);

        artifactTransform.anchoredPosition =
            Vector2.zero;

        artifactTransform.localScale =
            Vector3.one;

        return true;
    }

    public void ClearArtifact()
    {
        artifact = null;
    }

    public void UpdateVisual()
    {
        if (blockedVisual != null)
            blockedVisual.SetActive(!IsUnlocked);
    }
}