using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BoardEditor))]
public class BoardEditorEditor : Editor
{
    private BoardEditor boardEditor;

    private void OnEnable()
    {
        boardEditor = (BoardEditor)target;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space();

        if (GUILayout.Button("Clear Board"))
        {
            Undo.RecordObject(boardEditor, "Clear Board");
            boardEditor.Clear();
            ClearEditorCells();
            EditorUtility.SetDirty(boardEditor);
        }
    }

    private void OnSceneGUI()
    {
        if (boardEditor.Grid == null)
            return;

        HandleInput();
    }

    private void HandleInput()
    {
        Event currentEvent = Event.current;

        if (currentEvent.type != EventType.MouseDown)
            return;

        if (currentEvent.button != 0)
            return;

        if (currentEvent.alt)
            return;

        Ray ray = HandleUtility.GUIPointToWorldRay(
            currentEvent.mousePosition
        );

        Plane plane = new Plane(
            Vector3.forward,
            boardEditor.transform.position
        );

        if (!plane.Raycast(ray, out float distance))
            return;

        Vector3 worldPosition = ray.GetPoint(distance);

        Vector3Int cellPosition =
            boardEditor.Grid.WorldToCell(worldPosition);

        Vector2Int coordinate = new(
            cellPosition.x,
            cellPosition.y
        );

        bool wasSelected =
            boardEditor.Contains(coordinate);

        Undo.RecordObject(boardEditor, "Edit Board");

        boardEditor.ToggleCoordinate(coordinate);

        if (wasSelected)
            RemoveEditorCell(coordinate);
        else
            CreateEditorCell(coordinate);

        EditorUtility.SetDirty(boardEditor);

        currentEvent.Use();
    }

    private void CreateEditorCell(Vector2Int coordinate)
    {
        if (boardEditor.EditorCellPrefab == null)
            return;

        if (boardEditor.EditorCellsParent == null)
            return;

        GameObject editorCell =
            PrefabUtility.InstantiatePrefab(
                boardEditor.EditorCellPrefab,
                boardEditor.EditorCellsParent
            ) as GameObject;

        if (editorCell == null)
            return;

        editorCell.transform.position =
            boardEditor.Grid.GetCellCenterWorld(
                new Vector3Int(
                    coordinate.x,
                    coordinate.y,
                    0
                )
            );
    }

    private void RemoveEditorCell(Vector2Int coordinate)
    {
        if (boardEditor.EditorCellsParent == null)
            return;

        Square[] squares =
            boardEditor.EditorCellsParent
                .GetComponentsInChildren<Square>();

        foreach (Square square in squares)
        {
            Vector3Int cellPosition =
                boardEditor.Grid.WorldToCell(
                    square.transform.position
                );

            Vector2Int squareCoordinate = new(
                cellPosition.x,
                cellPosition.y
            );

            if (squareCoordinate != coordinate)
                continue;

            Undo.DestroyObjectImmediate(
                square.gameObject
            );

            return;
        }
    }

    private void ClearEditorCells()
    {
        if (boardEditor.EditorCellsParent == null)
            return;

        for (int i =
             boardEditor.EditorCellsParent.childCount - 1;
             i >= 0;
             i--)
        {
            Undo.DestroyObjectImmediate(
                boardEditor.EditorCellsParent.GetChild(i).gameObject
            );
        }
    }
}