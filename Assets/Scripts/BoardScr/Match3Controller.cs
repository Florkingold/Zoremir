
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Match3Controller : MonoBehaviour
{
    [SerializeField] private Grid grid;
    [SerializeField] private GameObject[] cellPrefabs;
    [SerializeField] private RoadController roadController;
    [SerializeField] private float destroyDuration = 2f;
    [SerializeField] private float fallDuration = 0.2f;

    private readonly Dictionary<Vector2Int, Square> board = new();

    private void Start()
    {
        SetupBoard();
        FillBoard();
        StartCoroutine(ProcessMatches());
    }

    private void SetupBoard()
    {
        board.Clear();

        Square[] squares = FindObjectsOfType<Square>();

        foreach (Square square in squares)
        {
            Vector3Int cellPosition =
                grid.WorldToCell(square.transform.position);

            Vector2Int coordinate = new(
                cellPosition.x,
                cellPosition.y
            );

            board[coordinate] = square;
        }
    }

    private void FillBoard()
    {
        foreach (KeyValuePair<Vector2Int, Square> entry in board)
        {
            if (!entry.Value.isEmpty)
                continue;

            SpawnCell(entry.Value);
        }
    }

    private void SpawnCell(Square square)
    {
        int typeIndex = Random.Range(0, cellPrefabs.Length);

        GameObject cellObject = Instantiate(
            cellPrefabs[typeIndex],
            square.transform.position,
            Quaternion.identity
        );

        Cell cell = cellObject.GetComponent<Cell>();

        square.cell = cell;
        square.isEmpty = false;
        cell.isRuined = false;
    }

    public void TrySwap(Cell selectedCell, Vector2Int direction)
    {
        if (selectedCell == null)
            return;

        if (!TryFindCellCoordinate(
                selectedCell,
                out Vector2Int selectedCoordinate))
            return;

        if (!board.TryGetValue(
                selectedCoordinate,
                out Square selectedSquare))
            return;

        Vector2Int targetCoordinate =
            selectedCoordinate + direction;

        if (!board.TryGetValue(
                targetCoordinate,
                out Square targetSquare))
            return;

        if (targetSquare.isEmpty ||
            targetSquare.cell == null)
            return;

        Cell targetCell = targetSquare.cell;

        selectedSquare.cell = targetCell;
        targetSquare.cell = selectedCell;

        selectedCell.transform.position =
            targetSquare.transform.position;

        targetCell.transform.position =
            selectedSquare.transform.position;

        StartCoroutine(
            CheckSwapResult(
                selectedSquare,
                targetSquare,
                selectedCell,
                targetCell
            )
        );
    }

    private bool TryFindCellCoordinate(
        Cell cell,
        out Vector2Int coordinate)
    {
        foreach (KeyValuePair<Vector2Int, Square> entry in board)
        {
            if (entry.Value.cell != cell)
                continue;

            coordinate = entry.Key;
            return true;
        }

        coordinate = Vector2Int.zero;
        return false;
    }

    private IEnumerator CheckSwapResult(
        Square firstSquare,
        Square secondSquare,
        Cell firstCell,
        Cell secondCell)
    {
        yield return new WaitForSeconds(0.1f);

        List<Cell> matches = FindMatches();

        if (matches.Count == 0)
        {
            firstSquare.cell = firstCell;
            secondSquare.cell = secondCell;

            if (firstCell != null)
            {
                firstCell.transform.position =
                    firstSquare.transform.position;
            }

            if (secondCell != null)
            {
                secondCell.transform.position =
                    secondSquare.transform.position;
            }

            yield break;
        }

        yield return StartCoroutine(DestroyMatches(matches));
        yield return StartCoroutine(ApplyGravity());

        FillBoard();

        yield return StartCoroutine(ProcessMatches());
    }

    private IEnumerator ProcessMatches()
    {
        yield return null;

        while (true)
        {
            List<Cell> matches = FindMatches();

            if (matches.Count == 0)
                yield break;

            yield return StartCoroutine(
                DestroyMatches(matches)
            );

            yield return StartCoroutine(
                ApplyGravity()
            );

            FillBoard();

            yield return null;
        }
    }

    private List<Cell> FindMatches()
    {
        HashSet<Cell> matches = new();

        foreach (KeyValuePair<Vector2Int, Square> entry in board)
        {
            Vector2Int coordinate = entry.Key;
            Square square = entry.Value;

            if (square.isEmpty ||
                square.cell == null)
                continue;

            Cell cell = square.cell;

            List<Cell> horizontal = GetLine(
                coordinate,
                Vector2Int.right,
                cell.type
            );

            if (horizontal.Count >= 3)
            {
                foreach (Cell match in horizontal)
                    matches.Add(match);
            }

            List<Cell> vertical = GetLine(
                coordinate,
                Vector2Int.up,
                cell.type
            );

            if (vertical.Count >= 3)
            {
                foreach (Cell match in vertical)
                    matches.Add(match);
            }
        }

        return new List<Cell>(matches);
    }

    private List<Cell> GetLine(
        Vector2Int start,
        Vector2Int direction,
        Cell.CellType type)
    {
        List<Cell> result = new();

        Vector2Int current = start;

        while (board.TryGetValue(
            current,
            out Square square))
        {
            if (square.isEmpty ||
                square.cell == null)
                break;

            if (square.cell.type != type)
                break;

            result.Add(square.cell);
            current += direction;
        }

        return result;
    }

    private IEnumerator DestroyMatches(
        List<Cell> matches)
    {
        foreach (Cell cell in matches)
        {
            if (cell != null)
                cell.isRuined = true;
        }

        foreach (Cell cell in matches)
        {
            if (cell != null)
                StartCoroutine(FadeAndDestroy(cell));
        }

        yield return new WaitForSeconds(
            destroyDuration
        );
    }

    private IEnumerator FadeAndDestroy(Cell cell)
    {
        if (cell == null)
            yield break;

        foreach (KeyValuePair<Vector2Int, Square> entry in board)
        {
            if (entry.Value.cell != cell)
                continue;

            if (roadController != null)
                roadController.RevealRoad(entry.Key);

            break;
        }

        SpriteRenderer spriteRenderer =
            cell.GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
        {
            RemoveCell(cell);
            yield break;
        }

        Color startColor = spriteRenderer.color;
        Color endColor = startColor;
        endColor.a = 0f;

        float time = 0f;

        while (time < destroyDuration)
        {
            if (cell == null ||
                spriteRenderer == null)
                yield break;

            time += Time.deltaTime;

            float progress =
                Mathf.Clamp01(
                    time / destroyDuration
                );

            spriteRenderer.color =
                Color.Lerp(
                    startColor,
                    endColor,
                    progress
                );

            yield return null;
        }

        if (cell != null)
            RemoveCell(cell);
    }

    private void RemoveCell(Cell cell)
    {
        if (cell == null)
            return;

        foreach (KeyValuePair<Vector2Int, Square> entry in board)
        {
            Square square = entry.Value;

            if (square.cell != cell)
                continue;

            square.cell = null;
            square.isEmpty = true;

            Destroy(cell.gameObject);

            return;
        }
    }

    private IEnumerator ApplyGravity()
    {
        Dictionary<
            int,
            List<KeyValuePair<Vector2Int, Square>>
        > columns = new();

        foreach (KeyValuePair<Vector2Int, Square> entry in board)
        {
            int x = entry.Key.x;

            if (!columns.ContainsKey(x))
            {
                columns[x] =
                    new List<KeyValuePair<Vector2Int, Square>>();
            }

            columns[x].Add(entry);
        }

        foreach (List<KeyValuePair<Vector2Int, Square>> column
                 in columns.Values)
        {
            column.Sort(
                (a, b) =>
                    a.Key.y.CompareTo(b.Key.y)
            );

            List<Square> occupied = new();

            foreach (KeyValuePair<Vector2Int, Square> entry
                     in column)
            {
                if (!entry.Value.isEmpty &&
                    entry.Value.cell != null)
                {
                    occupied.Add(entry.Value);
                }
            }

            for (int i = 0; i < column.Count; i++)
            {
                Square targetSquare =
                    column[i].Value;

                if (i < occupied.Count)
                {
                    Square sourceSquare =
                        occupied[i];

                    if (sourceSquare == targetSquare)
                        continue;

                    Cell cell =
                        sourceSquare.cell;

                    sourceSquare.cell = null;
                    sourceSquare.isEmpty = true;

                    targetSquare.cell = cell;
                    targetSquare.isEmpty = false;

                    if (cell != null)
                    {
                        StartCoroutine(
                            MoveCell(
                                cell,
                                targetSquare.transform.position
                            )
                        );
                    }
                }
                else
                {
                    targetSquare.cell = null;
                    targetSquare.isEmpty = true;
                }
            }
        }

        yield return new WaitForSeconds(
            fallDuration
        );
    }

    private IEnumerator MoveCell(
        Cell cell,
        Vector3 targetPosition)
    {
        if (cell == null)
            yield break;

        Vector3 startPosition =
            cell.transform.position;

        float time = 0f;

        while (time < fallDuration)
        {
            if (cell == null)
                yield break;

            time += Time.deltaTime;

            float progress =
                Mathf.Clamp01(
                    time / fallDuration
                );

            cell.transform.position =
                Vector3.Lerp(
                    startPosition,
                    targetPosition,
                    progress
                );

            yield return null;
        }

        if (cell == null)
            yield break;

        cell.transform.position =
            targetPosition;
    }
}

