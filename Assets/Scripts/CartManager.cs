using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Collections;

public class CartManager : MonoBehaviour
{
    [Header("Referências da UI")]
    public TextMeshProUGUI priceText;

    [Header("Posicionamento dos Itens")]
    public Transform cartInside;
    public float stackYOffset = 0.1f;
    public float stackXSpacing = 0.5f; // Aumentei o valor padrão
    public float stackZSpacing = 0.5f; // Aumentei o valor padrão

    public float totalPrice = 0f;
    public List<GameObject> collectedProducts = new List<GameObject>();

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
        collectedProducts.Add(productObject);

        // 1. SOLTA NO MUNDO (Não é filho do carrinho ainda)
        productObject.transform.SetParent(null);

        // 2. LIGA A FÍSICA REAL
        Rigidbody rb = productObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false; // A gravidade puxa
            rb.velocity = Vector3.zero; 
        }

        Collider col = productObject.GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = true;
            col.isTrigger = false; // Fica sólido para não atravessar o carrinho
        }

        // 3. JOGA O ITEM LÁ DE CIMA PARA CAIR NO CESTO
        // Aumentamos o raio da queda para evitar que caiam exatamente no mesmo pixel
        float randomX = Random.Range(-0.4f, 0.4f); 
        float randomZ = Random.Range(-0.4f, 0.4f);
        productObject.transform.position = cartInside.position + new Vector3(randomX, 0.8f, randomZ);
        
        // Dá um giro aleatório para a pilha não ficar parecendo blocos de Tetris
        productObject.transform.rotation = Random.rotation; 

        // 4. INICIA O CRONÔMETRO DE CONGELAMENTO
        StartCoroutine(SettleAndFreeze(productObject));
    }

    // --- A NOVA ROTINA MÁGICA ---
    private IEnumerator SettleAndFreeze(GameObject item)
    {
        // Espera 1.5 segundos para a física organizar a pilha
        yield return new WaitForSeconds(1.5f);

        if (item != null && collectedProducts.Contains(item))
        {
            // 1. A CIRURGIA: Arranca o "cérebro físico" do item.
            // Isso quebra o paradoxo infinito instantaneamente.
            Rigidbody rb = item.GetComponent<Rigidbody>();
            if (rb != null) Destroy(rb);

            // 2. Mantém o colisor sólido! 
            // Assim os próximos itens que caírem vão bater nesta garrafa e empilhar nela.
            Collider col = item.GetComponent<Collider>();
            if (col != null) col.isTrigger = false; 

            // 3. Gruda no carrinho com total segurança.
            item.transform.SetParent(cartInside, true);
        }
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
                col.enabled = true;
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