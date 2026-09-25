using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Match3Controller : MonoBehaviour
{
    [SerializeField] private Grid grid;
    [SerializeField] private GameObject[] cellPrefabs;
    [SerializeField] private RoadController roadController;
    [SerializeField] private TMP_Text noMovesText;
    [SerializeField] private float noMovesDuration = 2f;
    [SerializeField] private float destroyDuration = 2f;
    [SerializeField] private float fallDuration = 0.2f;

    private readonly Dictionary<Vector2Int, Square> board = new();

    private bool isBusy;
    private bool unlockRefreshRequested;
    private bool unlockRefreshRunning;

    private void Start()
    {
        if (noMovesText != null)
            noMovesText.gameObject.SetActive(false);

        StartCoroutine(InitializeBoard());
    }

    private IEnumerator InitializeBoard()
    {
        isBusy = true;

        SetupBoard();
        FillBoard();

        yield return StartCoroutine(ProcessMatches());
        yield return StartCoroutine(EnsurePlayableBoard());

        isBusy = false;
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
            Square square = entry.Value;

            if (square.isBlocked)
            {
                square.cell = null;
                square.isEmpty = true;
                continue;
            }

            if (!square.isEmpty)
                continue;

            SpawnCell(square);
        }
    }

    private void SpawnCell(Square square)
    {
        if (square == null ||
            square.isBlocked)
            return;

        if (cellPrefabs == null ||
            cellPrefabs.Length == 0)
            return;

        int typeIndex =
            Random.Range(0, cellPrefabs.Length);

        GameObject cellObject = Instantiate(
            cellPrefabs[typeIndex],
            square.transform.position,
            Quaternion.identity
        );

        Cell cell =
            cellObject.GetComponent<Cell>();

        if (cell == null)
        {
            Destroy(cellObject);
            return;
        }

        square.cell = cell;
        square.isEmpty = false;
        cell.isRuined = false;
    }

    public void RefreshAfterUnlock()
    {
        unlockRefreshRequested = true;

        if (unlockRefreshRunning)
            return;

        StartCoroutine(
            ProcessUnlockRefresh()
        );
    }

    private IEnumerator ProcessUnlockRefresh()
    {
        unlockRefreshRunning = true;

        while (isBusy)
            yield return null;

        if (!unlockRefreshRequested)
        {
            unlockRefreshRunning = false;
            yield break;
        }

        unlockRefreshRequested = false;
        isBusy = true;

        yield return StartCoroutine(
            ApplyGravity()
        );

        FillBoard();

        yield return StartCoroutine(
            ProcessMatches()
        );

        yield return StartCoroutine(
            EnsurePlayableBoard()
        );

        isBusy = false;
        unlockRefreshRunning = false;

        if (unlockRefreshRequested)
            StartCoroutine(
                ProcessUnlockRefresh()
            );
    }

    public void TrySwap(
        Cell selectedCell,
        Vector2Int direction)
    {
        if (isBusy)
            return;

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

        if (selectedSquare.isBlocked)
            return;

        Vector2Int targetCoordinate =
            selectedCoordinate + direction;

        if (!board.TryGetValue(
                targetCoordinate,
                out Square targetSquare))
            return;

        if (targetSquare.isBlocked)
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

        isBusy = true;

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

            isBusy = false;
            yield break;
        }

        yield return StartCoroutine(
            DestroyMatches(matches)
        );

        yield return StartCoroutine(
            ApplyGravity()
        );

        FillBoard();

        yield return StartCoroutine(
            ProcessMatches()
        );

        yield return StartCoroutine(
            EnsurePlayableBoard()
        );

        isBusy = false;
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

    private IEnumerator EnsurePlayableBoard()
    {
        while (!HasPossibleMove())
        {
            if (noMovesText != null)
            {
                noMovesText.text = "No moves";
                noMovesText.gameObject.SetActive(true);
            }

            yield return new WaitForSeconds(
                noMovesDuration
            );

            if (noMovesText != null)
                noMovesText.gameObject.SetActive(false);

            ClearCells();
            FillBoard();

            yield return StartCoroutine(
                ProcessMatches()
            );
        }
    }

    private bool HasPossibleMove()
    {
        foreach (KeyValuePair<Vector2Int, Square> entry in board)
        {
            Vector2Int coordinate = entry.Key;
            Square square = entry.Value;

            if (square.isBlocked)
                continue;

            if (square.isEmpty ||
                square.cell == null)
                continue;

            if (HasPossibleMove(
                    coordinate,
                    Vector2Int.right))
                return true;

            if (HasPossibleMove(
                    coordinate,
                    Vector2Int.up))
                return true;
        }

        return false;
    }

    private bool HasPossibleMove(
        Vector2Int firstCoordinate,
        Vector2Int direction)
    {
        Vector2Int secondCoordinate =
            firstCoordinate + direction;

        if (!board.TryGetValue(
                secondCoordinate,
                out Square secondSquare))
            return false;

        if (secondSquare.isBlocked)
            return false;

        if (secondSquare.isEmpty ||
            secondSquare.cell == null)
            return false;

        if (!board.TryGetValue(
                firstCoordinate,
                out Square firstSquare))
            return false;

        if (firstSquare.isBlocked)
            return false;

        Cell firstCell =
            firstSquare.cell;

        Cell secondCell =
            secondSquare.cell;

        firstSquare.cell = secondCell;
        secondSquare.cell = firstCell;

        bool createsMatch =
            FindMatches().Count > 0;

        firstSquare.cell = firstCell;
        secondSquare.cell = secondCell;

        return createsMatch;
    }

    private void ClearCells()
    {
        foreach (KeyValuePair<Vector2Int, Square> entry in board)
        {
            Square square = entry.Value;

            if (square.isBlocked)
            {
                if (square.cell != null)
                {
                    Destroy(square.cell.gameObject);
                    square.cell = null;
                }

                square.isEmpty = true;
                continue;
            }

            if (square.cell == null)
            {
                square.isEmpty = true;
                continue;
            }

            Destroy(square.cell.gameObject);

            square.cell = null;
            square.isEmpty = true;
        }
    }

    private List<Cell> FindMatches()
    {
        HashSet<Cell> matches = new();

        foreach (KeyValuePair<Vector2Int, Square> entry in board)
        {
            Vector2Int coordinate = entry.Key;
            Square square = entry.Value;

            if (square.isBlocked)
                continue;

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
            if (square.isBlocked)
                break;

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
                StartCoroutine(
                    FadeAndDestroy(cell)
                );
        }

        yield return new WaitForSeconds(
            destroyDuration
        );
    }

    private IEnumerator FadeAndDestroy(
        Cell cell)
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

        Color startColor =
            spriteRenderer.color;

        Color endColor =
            startColor;

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

            List<Square> segment = new();

            foreach (KeyValuePair<Vector2Int, Square> entry
                     in column)
            {
                Square square = entry.Value;

                if (square.isBlocked)
                {
                    ApplyGravityToSegment(segment);
                    segment.Clear();
                    continue;
                }

                segment.Add(square);
            }

            ApplyGravityToSegment(segment);
        }

        yield return new WaitForSeconds(
            fallDuration
        );
    }

    private void ApplyGravityToSegment(
        List<Square> segment)
    {
        if (segment == null ||
            segment.Count == 0)
            return;

        List<Square> occupied = new();

        foreach (Square square in segment)
        {
            if (square.isBlocked)
                continue;

            if (!square.isEmpty &&
                square.cell != null)
            {
                occupied.Add(square);
            }
        }

        for (int i = 0; i < segment.Count; i++)
        {
            Square targetSquare =
                segment[i];

            if (targetSquare.isBlocked)
                continue;

            if (i < occupied.Count)
            {
                Square sourceSquare =
                    occupied[i];

                if (sourceSquare ==
                    targetSquare)
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