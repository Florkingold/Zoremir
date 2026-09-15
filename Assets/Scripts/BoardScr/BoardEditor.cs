
using System.Collections.Generic;
using UnityEngine;

public class BoardEditor : MonoBehaviour
{
    [SerializeField] private Grid grid;
    [SerializeField] private GameObject editorCellPrefab;
    [SerializeField] private Transform editorCellsParent;
    [SerializeField] private List<Vector2Int> boardCoordinates = new();

    public Grid Grid => grid;
    public GameObject EditorCellPrefab => editorCellPrefab;
    public Transform EditorCellsParent => editorCellsParent;
    public List<Vector2Int> BoardCoordinates => boardCoordinates;

    public bool Contains(Vector2Int coordinate)
    {
        return boardCoordinates.Contains(coordinate);
    }

    public void ToggleCoordinate(Vector2Int coordinate)
    {
        if (boardCoordinates.Contains(coordinate))
        {
            boardCoordinates.RemoveAll(value => value == coordinate);
            return;
        }

        boardCoordinates.Add(coordinate);
    }

    public void Clear()
    {
        boardCoordinates.Clear();
    }
}

