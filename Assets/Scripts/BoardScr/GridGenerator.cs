using System.Collections.Generic;
using UnityEngine;

public class GridGenerator : MonoBehaviour
{
    [SerializeField] private Grid grid;
    [SerializeField] private BoardEditor boardEditor;

    [SerializeField] private RoadEditor roadEditor;
    [SerializeField] private GameObject roadPrefab;
    [SerializeField] private Transform roadsParent;

    private readonly Dictionary<Vector2Int, Square> squares = new();
    private readonly Dictionary<Vector2Int, Road> roads = new();

    private void Awake()
    {
        RegisterSquares();
        GenerateRoad();
    }

    private void RegisterSquares()
    {
        if (grid == null || boardEditor == null)
            return;

        Square[] existingSquares =
            FindObjectsOfType<Square>();

        foreach (Square square in existingSquares)
        {
            Vector3Int cellPosition =
                grid.WorldToCell(square.transform.position);

            Vector2Int coordinate = new(
                cellPosition.x,
                cellPosition.y
            );

            if (!boardEditor.Contains(coordinate))
                continue;

            if (squares.ContainsKey(coordinate))
                continue;

            squares.Add(coordinate, square);
        }
    }

    private void GenerateRoad()
    {
        if (grid == null || roadEditor == null || roadPrefab == null || roadsParent == null)
            return;

        foreach (Vector2Int coordinate in roadEditor.RoadCoordinates)
        {
            if (roads.ContainsKey(coordinate))
                continue;

            Vector3 position = grid.GetCellCenterWorld(
                new Vector3Int(coordinate.x, coordinate.y, 0)
            );

            GameObject roadObject = Instantiate(
                roadPrefab,
                position,
                Quaternion.identity,
                roadsParent
            );

            Road road = roadObject.GetComponent<Road>();

            if (road == null)
                continue;

            roads.Add(coordinate, road);
        }
    }

    public Square GetSquare(Vector2Int coordinate)
    {
        squares.TryGetValue(coordinate, out Square square);
        return square;
    }

    public Road GetRoad(Vector2Int coordinate)
    {
        roads.TryGetValue(coordinate, out Road road);
        return road;
    }
}