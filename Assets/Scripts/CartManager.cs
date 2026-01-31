using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class CartManager : MonoBehaviour
{
    [Header("Referências da UI")]
    public TextMeshProUGUI priceText;

    [Header("Posicionamento dos Itens")]
    public Transform cartInside;
    public float stackYOffset = 0.1f;
    public float stackXSpacing = 0.5f; // Aumentei o valor padrão
    public float stackZSpacing = 0.5f; // Aumentei o valor padrão

    private float totalPrice = 0f;
    private List<GameObject> collectedProducts = new List<GameObject>();

    void Start()
    {
        UpdatePriceUI();
        if (cartInside == null)
        {
            cartInside = transform;
        }
    }

    public void AddProductToCart(GameObject productObject)
    {
        Product product = productObject.GetComponent<Product>();
        if (product == null || product.isCollected) return;

        product.isCollected = true;
        totalPrice += product.price;
        UpdatePriceUI();

        Rigidbody rb = productObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.detectCollisions = false;
        }
        
        // --- LINHA CORRIGIDA ---
        // Em vez de desativar o collider, nós o transformamos em um Trigger.
        // Assim, o Raycast ainda pode detectá-lo.
        Collider col = productObject.GetComponent<Collider>();
        if (col != null) col.isTrigger = true;

        collectedProducts.Add(productObject);
        productObject.transform.SetParent(cartInside, false);
        productObject.transform.localPosition = CalculateLocalPosition(collectedProducts.Count - 1);
        productObject.transform.localRotation = Quaternion.identity;
    }

    public void RemoveProductFromCart(GameObject productObject)
    {
        Product product = productObject.GetComponent<Product>();
        if (product == null) return;
        
        if (collectedProducts.Contains(productObject))
        {
            product.isCollected = false;
            totalPrice -= product.price;
            UpdatePriceUI();
            collectedProducts.Remove(productObject);
            // Restaurar físicas e colisores para permitir que o produto volte a interagir com o mundo
            Rigidbody rb = productObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.detectCollisions = true;
            }

            Collider col = productObject.GetComponent<Collider>();
            if (col != null)
            {
                col.isTrigger = false;
            }

            // Solta o objeto do carrinho
            productObject.transform.SetParent(null);
        }
    }

    Vector3 CalculateLocalPosition(int index)
    {
        int itemsPerRow = 3;
        int row = index / itemsPerRow;
        int col = index % itemsPerRow;
        float x = (col - (itemsPerRow - 1) / 2.0f) * stackXSpacing;
        float y = row * stackYOffset;
        float z = 0f;
        return new Vector3(x, y, z);
    }

    void UpdatePriceUI()
    {
        if (priceText != null)
        {
            priceText.text = "Total: R$ " + totalPrice.ToString("F2");
        }
        
        // Atualiza também o GameUI se existir
        if (GameUI.Instance != null)
        {
            GameUI.Instance.UpdateTotalPrice(totalPrice);
        }
    }

    // Métodos públicos para acesso aos dados
    public float GetTotalPrice()
    {
        return totalPrice;
    }

    public int GetItemCount()
    {
        return collectedProducts.Count;
    }

    public System.Collections.Generic.List<GameObject> GetCollectedProducts()
    {
        return new System.Collections.Generic.List<GameObject>(collectedProducts);
    }
}