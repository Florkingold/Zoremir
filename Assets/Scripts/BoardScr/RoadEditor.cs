using System.Collections.Generic;
using UnityEngine;

public class RoadEditor : MonoBehaviour
{
    [SerializeField] private Grid grid;
    [SerializeField] private List<Vector2Int> roadCoordinates = new();

    public Grid Grid => grid;
    public IReadOnlyList<Vector2Int> RoadCoordinates => roadCoordinates;

    public bool Contains(Vector2Int coordinate)
    {
        return roadCoordinates.Contains(coordinate);
    }

    public void ToggleCoordinate(Vector2Int coordinate)
    {
        if (roadCoordinates.Contains(coordinate))
            roadCoordinates.Remove(coordinate);
        else
            roadCoordinates.Add(coordinate);
    }

    public void Clear()
    {
        roadCoordinates.Clear();
    }
}