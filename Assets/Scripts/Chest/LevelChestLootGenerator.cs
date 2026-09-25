using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(100)]
public class LevelChestLootGenerator : MonoBehaviour
{
    [SerializeField, Min(0)]
    private int keyRewardCount;

    private void Start()
    {
        GenerateLoot();
    }

    private void GenerateLoot()
    {
        Chest[] chests =
            FindObjectsOfType<Chest>();

        if (chests.Length == 0)
            return;

        ArtifactPanel artifactPanel =
            FindObjectOfType<ArtifactPanel>();

        List<Artifact> activeArtifacts =
            GetActiveArtifacts(artifactPanel);

        int actualKeyRewardCount =
            Mathf.Clamp(
                keyRewardCount,
                0,
                chests.Length
            );

        List<int> chestIndices =
            new();

        for (int i = 0; i < chests.Length; i++)
            chestIndices.Add(i);

        Shuffle(chestIndices);

        for (int i = 0; i < chests.Length; i++)
        {
            Chest chest =
                chests[i];

            if (chest == null)
                continue;

            chest.ClearReward();
        }

        for (int i = 0; i < actualKeyRewardCount; i++)
        {
            Chest chest =
                chests[chestIndices[i]];

            if (chest == null)
                continue;

            chest.SetKeyReward();
        }

        if (activeArtifacts.Count == 0)
            return;

        for (int i = actualKeyRewardCount; i < chestIndices.Count; i++)
        {
            Chest chest =
                chests[chestIndices[i]];

            if (chest == null)
                continue;

            Artifact artifact =
                activeArtifacts[
                    Random.Range(
                        0,
                        activeArtifacts.Count
                    )
                ];

            chest.SetArtifactReward(artifact);
        }
    }

    private List<Artifact> GetActiveArtifacts(
        ArtifactPanel artifactPanel)
    {
        List<Artifact> activeArtifacts =
            new();

        if (artifactPanel == null)
            return activeArtifacts;

        ArtifactSlot[] slots =
            artifactPanel.GetComponentsInChildren<ArtifactSlot>(
                true
            );

        foreach (ArtifactSlot slot in slots)
        {
            if (slot == null)
                continue;

            if (!slot.IsOccupied)
                continue;

            if (slot.Artifact == null)
                continue;

            activeArtifacts.Add(slot.Artifact);
        }

        return activeArtifacts;
    }

    private void Shuffle(List<int> values)
    {
        for (int i = values.Count - 1; i > 0; i--)
        {
            int randomIndex =
                Random.Range(0, i + 1);

            int temporary =
                values[i];

            values[i] =
                values[randomIndex];

            values[randomIndex] =
                temporary;
        }
    }
}