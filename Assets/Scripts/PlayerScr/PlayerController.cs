using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Grid grid;
    [SerializeField] private float doubleClickTime = 0.3f;
    [SerializeField] private float moveDuration = 0.2f;

    private readonly Dictionary<Vector2Int, Road> roads = new();

    private float lastClickTime = -1f;
    private Player player;
    private bool isMoving;

    private void Awake()
    {
        player = GetComponent<Player>();
    }

    private void Start()
    {
        RegisterRoads();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
            HandleClick();
    }

    private void RegisterRoads()
    {
        roads.Clear();

        Road[] roadObjects =
            FindObjectsOfType<Road>(true);

        foreach (Road road in roadObjects)
        {
            Vector3Int cellPosition =
                grid.WorldToCell(road.transform.position);

            Vector2Int coordinate = new(
                cellPosition.x,
                cellPosition.y
            );

            roads[coordinate] = road;
        }
    }

    private void HandleClick()
    {
        float currentTime = Time.time;

        if (currentTime - lastClickTime > doubleClickTime)
        {
            lastClickTime = currentTime;
            return;
        }

        lastClickTime = -1f;

        if (isMoving)
            return;

        Vector3 worldPosition =
            GetMouseWorldPosition();

        Vector3Int cellPosition =
            grid.WorldToCell(worldPosition);

        Vector2Int targetCoordinate = new(
            cellPosition.x,
            cellPosition.y
        );

        TryMoveTo(targetCoordinate);
    }

    private void TryMoveTo(Vector2Int targetCoordinate)
    {
        Vector2Int playerCoordinate =
            GetPlayerCoordinate();

        if (!roads.TryGetValue(
                targetCoordinate,
                out Road targetRoad))
            return;

        if (!targetRoad.isInteractable)
            return;

        List<Vector2Int> path =
            FindPath(
                playerCoordinate,
                targetCoordinate
            );

        if (path == null || path.Count < 2)
            return;

        StartCoroutine(MoveAlongPath(path));
    }

    private List<Vector2Int> FindPath(
        Vector2Int start,
        Vector2Int target)
    {
        Queue<Vector2Int> queue = new();
        Dictionary<Vector2Int, Vector2Int> previous = new();
        HashSet<Vector2Int> visited = new();

        queue.Enqueue(start);
        visited.Add(start);

        Vector2Int[] directions =
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();

            if (current == target)
                break;

            foreach (Vector2Int direction in directions)
            {
                Vector2Int next =
                    current + direction;

                if (visited.Contains(next))
                    continue;

                if (!roads.TryGetValue(next, out Road road))
                    continue;

                if (!road.isInteractable)
                    continue;

                visited.Add(next);
                previous[next] = current;
                queue.Enqueue(next);
            }
        }

        if (!visited.Contains(target))
            return null;

        List<Vector2Int> path = new();
        Vector2Int currentPosition = target;

        while (currentPosition != start)
        {
            path.Add(currentPosition);
            currentPosition = previous[currentPosition];
        }

        path.Add(start);
        path.Reverse();

        return path;
    }

    private IEnumerator MoveAlongPath(
        List<Vector2Int> path)
    {
        isMoving = true;

        for (int i = 1; i < path.Count; i++)
        {
            Vector3 targetPosition =
                grid.GetCellCenterWorld(
                    new Vector3Int(
                        path[i].x,
                        path[i].y,
                        0
                    )
                );

            Vector3 startPosition =
                transform.position;

            float time = 0f;

            while (time < moveDuration)
            {
                time += Time.deltaTime;

                float progress =
                    Mathf.Clamp01(
                        time / moveDuration
                    );

                transform.position =
                    Vector3.Lerp(
                        startPosition,
                        targetPosition,
                        progress
                    );

                yield return null;
            }

            transform.position = targetPosition;
        }

        isMoving = false;
    }

    private Vector2Int GetPlayerCoordinate()
    {
        Vector3Int cellPosition =
            grid.WorldToCell(transform.position);

        return new Vector2Int(
            cellPosition.x,
            cellPosition.y
        );
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePosition =
            Input.mousePosition;

        mousePosition.z =
            Mathf.Abs(
                Camera.main.transform.position.z
            );

        return Camera.main.ScreenToWorldPoint(
            mousePosition
        );
    }
}