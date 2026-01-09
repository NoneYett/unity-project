using UnityEngine;

public class Product : MonoBehaviour
{
    public string productName = "Produto";
    public float price = 1.00f;

    [HideInInspector] public bool isCollected = false;
}
