using TMPro;
using UnityEngine;

public class Key : MonoBehaviour
{
    [SerializeField] private TMP_Text quantityText;

    private void Start()
    {
        if (quantityText == null)
            quantityText = GetComponentInChildren<TMP_Text>(true);

        if (PlayerKeyStorage.Instance == null)
            return;

        PlayerKeyStorage.Instance.OnKeyCountChanged += UpdateQuantity;

        UpdateQuantity(
            PlayerKeyStorage.Instance.KeyCount
        );
    }

    private void OnDestroy()
    {
        if (PlayerKeyStorage.Instance != null)
            PlayerKeyStorage.Instance.OnKeyCountChanged -= UpdateQuantity;
    }

    private void UpdateQuantity(int quantity)
    {
        if (quantityText == null)
            return;

        quantityText.text = quantity.ToString();
    }
}
