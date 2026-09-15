using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RoadEditor))]
public class RoadEditorEditor : Editor
{
    private RoadEditor roadEditor;

    private void OnEnable()
    {
        roadEditor = (RoadEditor)target;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space();

        if (GUILayout.Button("Clear Road"))
        {
            Undo.RecordObject(roadEditor, "Clear Road");
            roadEditor.Clear();
            EditorUtility.SetDirty(roadEditor);
        }
    }

    private void OnSceneGUI()
    {
        if (roadEditor.Grid == null)
            return;

        DrawRoad();
        HandleInput();
    }

    private void DrawRoad()
    {
        Vector3 cellSize = roadEditor.Grid.cellSize;

        foreach (Vector2Int coordinate in roadEditor.RoadCoordinates)
        {
            Vector3 center = roadEditor.Grid.GetCellCenterWorld(
                new Vector3Int(coordinate.x, coordinate.y, 0)
            );

            Vector3[] corners =
            {
                center + new Vector3(-cellSize.x, -cellSize.y, 0f) * 0.5f,
                center + new Vector3(-cellSize.x, cellSize.y, 0f) * 0.5f,
                center + new Vector3(cellSize.x, cellSize.y, 0f) * 0.5f,
                center + new Vector3(cellSize.x, -cellSize.y, 0f) * 0.5f
            };

            Handles.DrawSolidRectangleWithOutline(
                corners,
                new Color(0.45f, 0.25f, 0.1f, 0.45f),
                new Color(1f, 1f, 1f, 0.2f)
            );
        }
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
            roadEditor.transform.position
        );

        if (!plane.Raycast(ray, out float distance))
            return;

        Vector3 worldPosition = ray.GetPoint(distance);

        Vector3Int cellPosition =
            roadEditor.Grid.WorldToCell(worldPosition);

        Vector2Int coordinate = new Vector2Int(
            cellPosition.x,
            cellPosition.y
        );

        Undo.RecordObject(
            roadEditor,
            "Edit Road"
        );

        roadEditor.ToggleCoordinate(coordinate);

        EditorUtility.SetDirty(roadEditor);

        currentEvent.Use();
    }
}