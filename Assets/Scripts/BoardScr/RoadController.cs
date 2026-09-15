
using UnityEngine;

public class RoadController : MonoBehaviour
{
    [SerializeField] private GridGenerator gridGenerator;

    public void RevealRoad(Vector2Int coordinate)
    {
        if (gridGenerator == null)
            return;

        Road road = gridGenerator.GetRoad(coordinate);

        if (road == null)
            return;

        RoadVisibilityController visibilityController =
            road.GetComponent<RoadVisibilityController>();

        if (visibilityController == null)
            return;

        visibilityController.Reveal();
    }
}

