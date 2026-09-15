using UnityEngine;

public class InputController : MonoBehaviour
{
    public Camera mainCamera;
    public Match3Controller match3Controller;
    public float swipeThreshold = 0.3f;

    [HideInInspector]
    public Cell selectedCell;

    [HideInInspector]
    public Vector3 startMousePosition;

    [HideInInspector]
    public bool isDragging;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            StartDrag();
        }

        if (Input.GetMouseButton(0) && isDragging)
        {
            CheckDrag();
        }

        if (Input.GetMouseButtonUp(0))
        {
            EndDrag();
        }
    }

    private void StartDrag()
    {
        Vector3 worldPosition = GetMouseWorldPosition();

        Collider2D hit = Physics2D.OverlapPoint(worldPosition);

        if (hit == null)
            return;

        Cell cell = hit.GetComponent<Cell>();

        if (cell == null)
            return;

        selectedCell = cell;
        startMousePosition = worldPosition;
        isDragging = true;
    }

    private void CheckDrag()
    {
        if (selectedCell == null)
            return;

        Vector3 worldPosition = GetMouseWorldPosition();
        Vector3 difference = worldPosition - startMousePosition;

        if (difference.magnitude < swipeThreshold)
            return;

        Vector2Int direction;

        if (Mathf.Abs(difference.x) > Mathf.Abs(difference.y))
        {
            direction = difference.x > 0
                ? Vector2Int.right
                : Vector2Int.left;
        }
        else
        {
            direction = difference.y > 0
                ? Vector2Int.up
                : Vector2Int.down;
        }

        match3Controller.TrySwap(selectedCell, direction);

        isDragging = false;
    }

    private void EndDrag()
    {
        selectedCell = null;
        isDragging = false;
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePosition = Input.mousePosition;

        mousePosition.z =
            Mathf.Abs(mainCamera.transform.position.z);

        return mainCamera.ScreenToWorldPoint(mousePosition);
    }
}