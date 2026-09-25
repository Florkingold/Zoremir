using System.Collections.Generic;
using UnityEngine;

public class Lock : MonoBehaviour
{
    [SerializeField, Range(1, 5)]
    private int lockId = 1;

    [SerializeField] private Grid grid;
    [SerializeField] private float doubleClickTime = 0.3f;

    private Square square;
    private float lastClickTime = -1f;
    private bool isOpened;

    public int LockId => lockId;
    public Square Square => square;
    public bool IsOpened => isOpened;

    private void Awake()
    {
        FindSquare();

        if (square != null)
            square.isBlocked = true;
    }

    private void OnMouseDown()
    {
        if (isOpened)
            return;

        float currentTime = Time.unscaledTime;

        if (currentTime - lastClickTime > doubleClickTime)
        {
            lastClickTime = currentTime;
            return;
        }

        lastClickTime = -1f;

        Open();
    }

    private void FindSquare()
    {
        if (grid == null)
            grid = FindFirstObjectByType<Grid>();

        if (grid == null)
            return;

        Vector3Int cellPosition =
            grid.WorldToCell(transform.position);

        Vector2Int coordinate = new(
            cellPosition.x,
            cellPosition.y
        );

        Square[] squares =
            FindObjectsOfType<Square>();

        foreach (Square candidate in squares)
        {
            if (candidate == null)
                continue;

            Vector3Int candidatePosition =
                grid.WorldToCell(
                    candidate.transform.position
                );

            Vector2Int candidateCoordinate = new(
                candidatePosition.x,
                candidatePosition.y
            );

            if (candidateCoordinate != coordinate)
                continue;

            square = candidate;
            return;
        }
    }

    private void Open()
    {
        if (square == null)
            return;

        if (PlayerKeyStorage.Instance == null)
            return;

        if (!PlayerKeyStorage.Instance.TryUseKey())
            return;

        isOpened = true;

        square.isBlocked = false;

        RemoveChains();

        Match3Controller match3Controller =
            FindFirstObjectByType<Match3Controller>();

        if (match3Controller != null)
            match3Controller.RefreshAfterUnlock();

        Collider2D collider2D =
            GetComponent<Collider2D>();

        if (collider2D != null)
            collider2D.enabled = false;

        Destroy(gameObject);
    }

    private void RemoveChains()
    {
        ChainGenerator chainGenerator =
            FindFirstObjectByType<ChainGenerator>();

        if (chainGenerator == null)
            return;

        List<Chain> chains =
            chainGenerator.GetChainsForLock(lockId);

        foreach (Chain chain in chains)
        {
            if (chain == null)
                continue;

            chain.Remove();
        }
    }
}