using UnityEngine;

public class PlayerHover : MonoBehaviour
{
    [SerializeField] private float hoverAlpha = 0.35f;

    private SpriteRenderer spriteRenderer;
    private Color normalColor;

    private void Awake()
    {
        spriteRenderer =
            GetComponent<SpriteRenderer>();

        normalColor =
            spriteRenderer.color;
    }

    private void OnMouseEnter()
    {
        Color color = normalColor;
        color.a = hoverAlpha;
        spriteRenderer.color = color;
    }

    private void OnMouseExit()
    {
        spriteRenderer.color = normalColor;
    }
}