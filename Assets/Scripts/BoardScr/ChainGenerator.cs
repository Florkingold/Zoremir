using System.Collections.Generic;
using UnityEngine;

public class ChainGenerator : MonoBehaviour
{
    [SerializeField] private Grid grid;
    [SerializeField] private GameObject chainPrefab;
    [SerializeField] private Transform chainsParent;

    private readonly Dictionary<Vector2Int, Chain> chains = new();

    public Grid Grid => grid;
    public GameObject ChainPrefab => chainPrefab;
    public Transform ChainsParent => chainsParent;

    private void Awake()
    {
        RegisterChains();
    }

    private void RegisterChains()
    {
        chains.Clear();

        if (grid == null || chainsParent == null)
            return;

        Chain[] existingChains =
            chainsParent.GetComponentsInChildren<Chain>(true);

        foreach (Chain chain in existingChains)
        {
            if (chain == null)
                continue;

            Vector3Int cellPosition =
                grid.WorldToCell(chain.transform.position);

            Vector2Int coordinate = new(
                cellPosition.x,
                cellPosition.y
            );

            Square square = GetSquare(coordinate);

            if (square == null)
                continue;

            chain.Initialize(square);

            if (!chains.ContainsKey(coordinate))
                chains.Add(coordinate, chain);
        }
    }

    public Square GetSquare(Vector2Int coordinate)
    {
        if (grid == null)
            return null;

        Square[] squares =
            FindObjectsOfType<Square>();

        foreach (Square square in squares)
        {
            if (square == null)
                continue;

            Vector3Int cellPosition =
                grid.WorldToCell(
                    square.transform.position
                );

            Vector2Int squareCoordinate = new(
                cellPosition.x,
                cellPosition.y
            );

            if (squareCoordinate == coordinate)
                return square;
        }

        return null;
    }

    public Chain GetChain(Vector2Int coordinate)
    {
        chains.TryGetValue(
            coordinate,
            out Chain chain
        );

        return chain;
    }

    public bool Contains(Vector2Int coordinate)
    {
        return chains.ContainsKey(coordinate);
    }

    public void RegisterChain(
        Vector2Int coordinate,
        Chain chain)
    {
        if (chain == null)
            return;

        chains[coordinate] = chain;
    }

    public void UnregisterChain(Vector2Int coordinate)
    {
        chains.Remove(coordinate);
    }

    public List<Chain> GetChainsForLock(int lockId)
    {
        List<Chain> result = new();

        foreach (Chain chain in chains.Values)
        {
            if (chain == null)
                continue;

            if (chain.LockId == lockId)
                result.Add(chain);
        }

        return result;
    }

    public void RefreshChains()
    {
        RegisterChains();
    }
}