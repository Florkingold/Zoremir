
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MagnifyingGlass : Artifact, IPointerClickHandler
{
    private float lastClickTime;
    private const float DoubleClickTime = 0.3f;
    private bool chargingProcessed;

    private void Update()
    {
        int ruinedCount = CountRuinedCells();

        if (ruinedCount == 4)
        {
            if (chargingProcessed)
                return;

            Charging(ruinedCount);
            chargingProcessed = true;
            return;
        }

        chargingProcessed = false;
    }

    public override void Charging(int destroyedCount)
    {
        if (destroyedCount != 4)
            return;

        AddArtifact();
    }

    protected override bool OnUse()
    {
        if (MainCamera == null)
            return false;

        List<RoadVisibilityController> availableRoads =
            GetAvailableRoads();

        if (availableRoads.Count == 0)
            return false;

        int randomIndex =
            Random.Range(0, availableRoads.Count);

        availableRoads[randomIndex].Reveal();

        return true;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        float currentTime = Time.unscaledTime;

        if (currentTime - lastClickTime <= DoubleClickTime)
        {
            Use();
            lastClickTime = 0f;
            return;
        }

        lastClickTime = currentTime;
    }

    private List<RoadVisibilityController> GetAvailableRoads()
    {
        RoadVisibilityController[] controllers =
            FindObjectsOfType<RoadVisibilityController>();

        List<RoadVisibilityController> availableRoads =
            new();

        foreach (RoadVisibilityController controller in controllers)
        {
            if (controller == null)
                continue;

            Road road =
                controller.GetComponent<Road>();

            if (road == null)
                continue;

            if (road.isVisible)
                continue;

            Vector3 viewportPosition =
                MainCamera.WorldToViewportPoint(
                    controller.transform.position
                );

            if (viewportPosition.z <= 0f)
                continue;

            if (viewportPosition.x < 0f ||
                viewportPosition.x > 1f ||
                viewportPosition.y < 0f ||
                viewportPosition.y > 1f)
                continue;

            availableRoads.Add(controller);
        }

        return availableRoads;
    }

    private int CountRuinedCells()
    {
        Cell[] cells =
            FindObjectsOfType<Cell>();

        int count = 0;

        foreach (Cell cell in cells)
        {
            if (cell == null)
                continue;

            if (cell.isRuined)
                count++;
        }

        return count;
    }
}

