using UnityEngine;

public class RoadVisibilityController : MonoBehaviour
{
    private Road road;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        road = GetComponent<Road>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        ApplyVisibility();
    }

    private void ApplyVisibility()
    {
        if (road == null || spriteRenderer == null)
            return;

        spriteRenderer.enabled = road.isVisible;
    }

    public void Reveal()
    {
        if (road == null || spriteRenderer == null)
            return;

        road.isVisible = true;
        road.isInteractable = true;

        spriteRenderer.enabled = true;
    }
}