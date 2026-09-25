using UnityEngine;

public class LevelArtifactProvider : MonoBehaviour
{
    [SerializeField] private GameObject artifactPrefab;

    private void Start()
    {
        GiveArtifact();
    }

    private void GiveArtifact()
    {
        ArtifactPanel artifactPanel =
            FindObjectOfType<ArtifactPanel>();

        if (artifactPanel == null)
            return;

        if (artifactPrefab == null)
            return;

        ArtifactSlot firstSlot =
            artifactPanel.GetSlot(0);

        if (firstSlot == null)
            return;

        if (!firstSlot.IsUnlocked)
            return;

        if (firstSlot.IsOccupied)
            return;

        GameObject artifactObject =
            Instantiate(artifactPrefab);

        Artifact artifact =
            artifactObject.GetComponent<Artifact>();

        if (artifact == null)
        {
            Destroy(artifactObject);
            return;
        }

        if (!firstSlot.SetArtifact(artifact))
        {
            Destroy(artifactObject);
        }
    }
}