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

    // Faz o item ser filho do carrinho para andar junto com ele
    productObject.transform.SetParent(cartInside, true);

    // LIGA A FÍSICA PARA O ITEM CAIR E BATER NOS OUTROS
    Rigidbody rb = productObject.GetComponent<Rigidbody>();
    if (rb != null)
    {
        rb.isKinematic = false; // A gravidade puxa ele
        rb.detectCollisions = true; // Ele bate nos outros itens
        
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic; // Impede o tunelamento
        rb.interpolation = RigidbodyInterpolation.Interpolate; // Deixa o movimento suave junto com o carrinho

        // Zera a força que ele tinha na sua mão para ele não ser arremessado
        rb.velocity = Vector3.zero; 
        rb.angularVelocity = Vector3.zero;
    }

    // Deixa o colisor sólido (não fantasma)
    Collider col = productObject.GetComponent<Collider>();
    if (col != null) col.isTrigger = false; 

    collectedProducts.Add(productObject);

    // O "PULO DO GATO": Solta o item um pouco acima do fundo do carrinho, 
    // com uma leve variação aleatória no X e Z para eles não caírem empilhados como uma torre perfeita.
    float randomX = Random.Range(-0.2f, 0.2f);
    float randomZ = Random.Range(-0.2f, 0.2f);
    productObject.transform.localPosition = new Vector3(randomX, 1.0f, randomZ);

    // Garante a escala que você está usando
    //productObject.transform.localScale = new Vector3(9f, 9f, 9f); 
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
    }

    public float GetTotalPrice()
    {
        return totalPrice;
    }

    public int GetItemCount()
    {
        return collectedProducts.Count;
    }
}