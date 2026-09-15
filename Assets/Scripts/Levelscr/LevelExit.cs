
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelExit : MonoBehaviour
{
    [SerializeField] private string targetScene;

    private bool isLoading;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isLoading)
            return;

        if (!other.CompareTag("Player"))
            return;

        if (string.IsNullOrWhiteSpace(targetScene))
            return;

        isLoading = true;
        SceneManager.LoadScene(targetScene);
    }
}

