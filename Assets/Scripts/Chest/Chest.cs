using UnityEngine;

public class Chest : MonoBehaviour
{
    [SerializeField] private SpriteRenderer closedVisual;
    [SerializeField] private SpriteRenderer openVisual;
    [SerializeField] private Collider2D triggerCollider;

    private int rewardType;
    private Artifact rewardArtifact;
    private bool isOpened;

    private const int NoReward = 0;
    private const int KeyReward = 1;
    private const int ArtifactReward = 2;

    public Artifact RewardArtifact => rewardArtifact;
    public bool IsOpened => isOpened;

    private void Awake()
    {
        if (triggerCollider == null)
            triggerCollider = GetComponent<Collider2D>();

        ApplyClosedVisual();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("CHEST TRIGGER: " + other.name, this);

        if (isOpened)
            return;

        Player player =
            other.GetComponentInParent<Player>();

        if (player == null)
            return;

        Debug.Log("CHEST FOUND PLAYER", this);

        Open();
    }

    public void SetKeyReward()
    {
        rewardType = KeyReward;
        rewardArtifact = null;
    }

    public void SetArtifactReward(Artifact artifact)
    {
        if (artifact == null)
        {
            ClearReward();
            return;
        }

        rewardType = ArtifactReward;
        rewardArtifact = artifact;
    }

    public void ClearReward()
    {
        rewardType = NoReward;
        rewardArtifact = null;
    }

    private void Open()
    {
        Debug.Log("CHEST OPEN", this);

        if (isOpened)
            return;

        isOpened = true;

        GiveReward();
        ApplyOpenVisual();

        if (triggerCollider != null)
            triggerCollider.enabled = false;
    }

    private void GiveReward()
    {
        switch (rewardType)
        {
            case KeyReward:
                GiveKey();
                break;

            case ArtifactReward:
                GiveArtifact();
                break;
        }
    }

    private void GiveKey()
    {
        if (PlayerKeyStorage.Instance == null)
            return;

        PlayerKeyStorage.Instance.AddKey();
    }

    private void GiveArtifact()
    {
        if (rewardArtifact == null)
            return;

        rewardArtifact.AddArtifact();
    }

    private void ApplyClosedVisual()
    {
        if (closedVisual != null)
            closedVisual.enabled = true;

        if (openVisual != null)
            openVisual.enabled = false;
    }

    private void ApplyOpenVisual()
    {
        if (closedVisual != null)
            closedVisual.enabled = false;

        if (openVisual != null)
            openVisual.enabled = true;
    }
}