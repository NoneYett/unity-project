using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

public class CheckoutManager : MonoBehaviour
{
    [Header("UI Elementos")]
    public GameObject checkoutPanel;
    public TextMeshProUGUI totalText;
    public TextMeshProUGUI itemsCountText;
    public TextMeshProUGUI scoreText;
    public Transform itemsListContainer;
    public GameObject itemPrefab;

    [Header("Referências")]
    public CartManager cartManager;

    [Header("Nova UI do Cupom")]
    public TextMeshProUGUI receiptText;

    // Classe auxiliar para guardar a matemática de cada produto
    private class ItemAgrupado
    {
        public string nome;
        public int quantidade;
        public float precoUnidade;
        public float precoTotal;
    }

    private bool isCheckoutActive = false;
    private bool hasCompletedCheckout = false;

    void Start()
    {
        if (checkoutPanel != null)
            checkoutPanel.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        // Se a trava estiver ativa (acabou de pagar ou fechou a tela), ignora a colisão
        if (hasCompletedCheckout) return;

        CartManager detectado = other.GetComponentInParent<CartManager>();
        
        if (detectado != null)
        {
            cartManager = detectado;

            // BUG 2 RESOLVIDO: Freio de mão! Zera a inércia do Rigidbody na hora
            Rigidbody rb = cartManager.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            OpenCheckout();
        }
    }

    void OnTriggerExit(Collider other)
    {
        CartManager detectado = other.GetComponentInParent<CartManager>();
        
        if (detectado != null)
        {
            // DESTRAVA O CAIXA: O carrinho finalmente saiu da área
            hasCompletedCheckout = false; 

            if (detectado == cartManager)
            {
                CloseCheckout();
                cartManager = null;
            }
        }
    }

    public void OpenCheckout()
    {
        if (isCheckoutActive) return;
        
        isCheckoutActive = true;
        
        if (checkoutPanel != null)
            checkoutPanel.SetActive(true);
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        Time.timeScale = 0f; // Pausa o jogo
        
        UpdateCheckoutUI();
    }

    public void CloseCheckout()
    {
        if (!isCheckoutActive) return;
        
        isCheckoutActive = false;
        
        if (checkoutPanel != null)
            checkoutPanel.SetActive(false);
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        Time.timeScale = 1f; // Despausa o jogo
    }

    void UpdateCheckoutUI()
    {
        if (cartManager == null) return;

        // Agrupa os itens iguais
        Dictionary<string, ItemAgrupado> itensComprados = new Dictionary<string, ItemAgrupado>();

        foreach (GameObject obj in cartManager.collectedProducts)
        {
            Product produto = obj.GetComponent<Product>();
            if (produto != null)
            {
                if (itensComprados.ContainsKey(produto.productName))
                {
                    itensComprados[produto.productName].quantidade++;
                    itensComprados[produto.productName].precoTotal += produto.price;
                }
                else
                {
                    itensComprados.Add(produto.productName, new ItemAgrupado
                    {
                        nome = produto.productName,
                        quantidade = 1,
                        precoUnidade = produto.price,
                        precoTotal = produto.price
                    });
                }
            }
        }

        // Visual do Cupom Fiscal
        string textoDoCupom = "CUPOM FISCAL\n----------------------\n";

        foreach (var item in itensComprados.Values)
        {
            textoDoCupom += $"{item.quantidade}x {item.nome} (R$ {item.precoUnidade:F2}) = R$ {item.precoTotal:F2}\n";
        }

        if (receiptText != null)
        {
            receiptText.text = textoDoCupom;
        }

        if (totalText != null)
        {
            totalText.text = "TOTAL A PAGAR: R$ " + cartManager.totalPrice.ToString("F2");
        }
    }

    public void FinishShopping()
    {
        if (cartManager != null)
        {
            foreach (GameObject item in cartManager.collectedProducts)
            {
                Destroy(item);
            }
            
            cartManager.collectedProducts.Clear();
            cartManager.totalPrice = 0f;

            // BUG 1 RESOLVIDO: Atualiza a interface do HUD do jogador para zero
            if (cartManager.priceText != null)
            {
                cartManager.priceText.text = "Total: R$ 0.00";
            }
        }

        // BUG 2 RESOLVIDO: Trava o caixa para a tela não abrir de novo imediatamente
        hasCompletedCheckout = true; 
        CloseCheckout();
    }

    public void ContinueShopping()
    {
        // Trava o caixa também se ele só fechar a tela sem pagar
        hasCompletedCheckout = true; 
        CloseCheckout();
    }
}