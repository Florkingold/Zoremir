using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform player;

    private void LateUpdate()
    {
        if (player == null)
            return;

        Vector3 position = player.position;
        position.z = transform.position.z;

        transform.position = position;
    }
}
