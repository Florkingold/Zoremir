using UnityEngine;

public class Chain : MonoBehaviour
{
    [SerializeField, Range(1, 5)]
    private int lockId = 1;

    private Square square;

    public int LockId => lockId;
    public Square Square => square;

    public void Initialize(Square targetSquare)
    {
        if (targetSquare == null)
            return;

        square = targetSquare;
        square.isBlocked = true;
    }

    public void SetLockId(int targetLockId)
    {
        lockId = Mathf.Clamp(targetLockId, 1, 5);
    }

    public void Remove()
    {
        if (square != null)
            square.isBlocked = false;

        Destroy(gameObject);
    }
}