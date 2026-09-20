using TMPro;
using UnityEngine;

public class ArtifactQuantityText : MonoBehaviour
{
    [SerializeField] private TMP_Text quantityText;

    private Artifact artifact;

    private void Start()
    {
        if (quantityText == null)
            quantityText = GetComponent<TMP_Text>();

        artifact = GetComponentInParent<Artifact>();

        if (artifact == null)
        {
            UpdateQuantity(0);
            return;
        }

        artifact.OnQuantityChanged += UpdateQuantity;
        UpdateQuantity(artifact.Quantity);
    }

    private void OnDestroy()
    {
        if (artifact != null)
            artifact.OnQuantityChanged -= UpdateQuantity;
    }

    private void UpdateQuantity(int quantity)
    {
        if (quantityText == null)
            return;

        quantityText.text = quantity.ToString();
    }
}