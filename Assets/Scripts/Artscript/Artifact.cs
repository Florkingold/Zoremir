using System;
using UnityEngine;

public abstract class Artifact : MonoBehaviour
{
    [SerializeField] private int quantity = 0;

    public static int MaxQuantity { get; private set; } = 5;

    public int Quantity => quantity;

    protected Camera MainCamera => Camera.main;

    public event Action<int> OnQuantityChanged;

    public abstract void Charging(int destroyedCount);

    public bool Use()
    {
        if (quantity <= 0)
            return false;

        if (!OnUse())
            return false;

        quantity--;
        OnQuantityChanged?.Invoke(quantity);

        return true;
    }

    protected abstract bool OnUse();

    public bool AddArtifact()
    {
        if (quantity >= MaxQuantity)
            return false;

        quantity++;
        OnQuantityChanged?.Invoke(quantity);

        return true;
    }

    public static void IncreaseMaxQuantity(int amount = 1)
    {
        if (amount <= 0)
            return;

        MaxQuantity += amount;
    }
}