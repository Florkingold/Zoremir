
using UnityEngine;

public class ClosePanelButton : MonoBehaviour
{
    [SerializeField] private GameObject panel;

    public void ClosePanel()
    {
        if (panel == null)
            return;

        panel.SetActive(false);
    }
}


