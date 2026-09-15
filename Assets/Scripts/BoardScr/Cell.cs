using UnityEngine;

public class Cell : MonoBehaviour
{
    public enum CellType
    {
        Red,
        Orange,
        Yellow,
        Green,
        Blue,
        Indigo,
        Violet
    }

    public CellType type;
    public bool isRuined = false;
}