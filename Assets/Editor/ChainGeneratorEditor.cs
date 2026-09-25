using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ChainGenerator))]
public class ChainGeneratorEditor : Editor
{
    private ChainGenerator chainGenerator;
    private Chain selectedChain;

    private void OnEnable()
    {
        chainGenerator = (ChainGenerator)target;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space();

        if (selectedChain != null)
        {
            DrawSelectedChain();
        }
        else
        {
            EditorGUILayout.HelpBox(
                "Select a chain in the Scene View.",
                MessageType.Info
            );
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("Clear Chains"))
        {
            ClearChains();
        }
    }

    private void DrawSelectedChain()
    {
        EditorGUILayout.LabelField(
            "Selected Chain",
            EditorStyles.boldLabel
        );

        Vector3Int cellPosition =
            chainGenerator.Grid.WorldToCell(
                selectedChain.transform.position
            );

        Vector2Int coordinate = new(
            cellPosition.x,
            cellPosition.y
        );

        EditorGUILayout.Vector2IntField(
            "Coordinate",
            coordinate
        );

        EditorGUI.BeginChangeCheck();

        int newLockId =
            EditorGUILayout.IntSlider(
                "Lock Id",
                selectedChain.LockId,
                1,
                5
            );

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(
                selectedChain,
                "Change Chain Lock Id"
            );

            selectedChain.SetLockId(newLockId);

            EditorUtility.SetDirty(selectedChain);
        }
    }

    private void OnSceneGUI()
    {
        if (chainGenerator.Grid == null)
            return;

        if (chainGenerator.ChainPrefab == null)
            return;

        if (chainGenerator.ChainsParent == null)
            return;

        Event currentEvent = Event.current;

        if (currentEvent.type == EventType.MouseDown &&
            currentEvent.button == 0 &&
            !currentEvent.alt)
        {
            HandleSceneClick(currentEvent);
            return;
        }

        if (currentEvent.type == EventType.MouseDown &&
            currentEvent.button == 0)
        {
            TrySelectChain(currentEvent);
        }
    }

    private void HandleSceneClick(Event currentEvent)
    {
        Ray ray =
            HandleUtility.GUIPointToWorldRay(
                currentEvent.mousePosition
            );

        Plane plane =
            new Plane(
                Vector3.forward,
                chainGenerator.transform.position
            );

        if (!plane.Raycast(
                ray,
                out float distance))
            return;

        Vector3 worldPosition =
            ray.GetPoint(distance);

        Vector3Int cellPosition =
            chainGenerator.Grid.WorldToCell(
                worldPosition
            );

        Vector2Int coordinate = new(
            cellPosition.x,
            cellPosition.y
        );

        Chain existingChain =
            FindChainAtCoordinate(coordinate);

        if (existingChain != null)
        {
            Undo.DestroyObjectImmediate(
                existingChain.gameObject
            );

            chainGenerator.UnregisterChain(
                coordinate
            );

            selectedChain = null;

            EditorUtility.SetDirty(
                chainGenerator
            );

            SceneView.RepaintAll();

            currentEvent.Use();

            return;
        }

        Square square =
            chainGenerator.GetSquare(coordinate);

        if (square == null)
        {
            currentEvent.Use();
            return;
        }

        GameObject chainObject =
            PrefabUtility.InstantiatePrefab(
                chainGenerator.ChainPrefab,
                chainGenerator.ChainsParent
            ) as GameObject;

        if (chainObject == null)
        {
            currentEvent.Use();
            return;
        }

        Undo.RegisterCreatedObjectUndo(
            chainObject,
            "Add Chain"
        );

        chainObject.transform.position =
            chainGenerator.Grid.GetCellCenterWorld(
                new Vector3Int(
                    coordinate.x,
                    coordinate.y,
                    0
                )
            );

        Chain chain =
            chainObject.GetComponent<Chain>();

        if (chain == null)
        {
            Undo.DestroyObjectImmediate(
                chainObject
            );

            currentEvent.Use();
            return;
        }

        chain.Initialize(square);

        chainGenerator.RegisterChain(
            coordinate,
            chain
        );

        selectedChain = chain;

        EditorUtility.SetDirty(
            chainGenerator
        );

        EditorUtility.SetDirty(
            chain
        );

        SceneView.RepaintAll();

        currentEvent.Use();
    }

    private void TrySelectChain(Event currentEvent)
    {
        Ray ray =
            HandleUtility.GUIPointToWorldRay(
                currentEvent.mousePosition
            );

        Plane plane =
            new Plane(
                Vector3.forward,
                chainGenerator.transform.position
            );

        if (!plane.Raycast(
                ray,
                out float distance))
            return;

        Vector3 worldPosition =
            ray.GetPoint(distance);

        Vector3Int cellPosition =
            chainGenerator.Grid.WorldToCell(
                worldPosition
            );

        Vector2Int coordinate = new(
            cellPosition.x,
            cellPosition.y
        );

        selectedChain =
            FindChainAtCoordinate(coordinate);

        if (selectedChain != null)
            Selection.activeGameObject =
                selectedChain.gameObject;

        SceneView.RepaintAll();
    }

    private Chain FindChainAtCoordinate(
        Vector2Int coordinate)
    {
        Chain[] existingChains =
            chainGenerator.ChainsParent
                .GetComponentsInChildren<Chain>(true);

        foreach (Chain chain in existingChains)
        {
            if (chain == null)
                continue;

            Vector3Int cellPosition =
                chainGenerator.Grid.WorldToCell(
                    chain.transform.position
                );

            Vector2Int chainCoordinate = new(
                cellPosition.x,
                cellPosition.y
            );

            if (chainCoordinate == coordinate)
                return chain;
        }

        return null;
    }

    private void ClearChains()
    {
        if (chainGenerator.ChainsParent == null)
            return;

        Undo.RecordObject(
            chainGenerator,
            "Clear Chains"
        );

        Chain[] existingChains =
            chainGenerator.ChainsParent
                .GetComponentsInChildren<Chain>(true);

        foreach (Chain chain in existingChains)
        {
            if (chain == null)
                continue;

            Undo.DestroyObjectImmediate(
                chain.gameObject
            );
        }

        chainGenerator.RefreshChains();

        selectedChain = null;

        EditorUtility.SetDirty(
            chainGenerator
        );

        SceneView.RepaintAll();
    }
}