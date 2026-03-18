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
    public TextMeshProUGUI receiptText; // Arraste o TextoListaCompras pra cá!

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
        if (hasCompletedCheckout) return;

        // Ao invés de usar Tag, ele detecta a existência do SEU script do carrinho
        CartManager detectado = other.GetComponentInParent<CartManager>();
        
        if (detectado != null)
        {
            cartManager = detectado; // O caixa "sequestra" o carrinho que entrou
            OpenCheckout();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (hasCompletedCheckout) return;

        CartManager detectado = other.GetComponentInParent<CartManager>();
        if (detectado != null && detectado == cartManager)
        {
            CloseCheckout();
            cartManager = null; // Limpa a memória quando o carrinho vai embora
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
        
        Time.timeScale = 0f;
        
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
        
        Time.timeScale = 1f;
    }

    void UpdateCheckoutUI()
    {
        if (cartManager == null) return;

        // 1. O DICIONÁRIO: Vai agrupar os itens iguais
        Dictionary<string, ItemAgrupado> itensComprados = new Dictionary<string, ItemAgrupado>();

        // Varre tudo que está fisicamente dentro do carrinho
        foreach (GameObject obj in cartManager.collectedProducts)
        {
            Product produto = obj.GetComponent<Product>();
            if (produto != null)
            {
                // Se o produto já está na lista, só aumenta a quantidade e o preço
                if (itensComprados.ContainsKey(produto.productName))
                {
                    itensComprados[produto.productName].quantidade++;
                    itensComprados[produto.productName].precoTotal += produto.price;
                }
                // Se é a primeira vez que acha esse produto, cria o registro dele
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

        // 2. O TEXTO: Montando o visual do Cupom Fiscal
        string textoDoCupom = "CUPOM FISCAL\n----------------------\n";

        foreach (var item in itensComprados.Values)
        {
            // Formato final: "3x Maçã (R$ 2.50) = R$ 7.50"
            textoDoCupom += $"{item.quantidade}x {item.nome} (R$ {item.precoUnidade:F2}) = R$ {item.precoTotal:F2}\n";
        }

        // 3. ENVIANDO PARA A TELA
        if (receiptText != null)
        {
            receiptText.text = textoDoCupom;
        }

        // Mantém o Total Geral funcionando lá embaixo
        if (totalText != null)
        {
            totalText.text = "TOTAL A PAGAR: R$ " + cartManager.totalPrice.ToString("F2");
        }
    }

    float GetCartTotal()
    {
        if (cartManager != null)
            return cartManager.totalPrice; // Lê a variável pública que já existe no seu CartManager
        return 0f;
    }

    int GetCartItemCount()
    {
        if (cartManager != null)
            return cartManager.collectedProducts.Count; // Conta o tamanho da sua lista de produtos
        return 0;
    }

    int CalculateScore()
    {
        // Pontuação baseada em itens coletados
        // Pode ser expandida para considerar lista de compras
        return GetCartItemCount() * 100;
    }

    public void FinishShopping()
    {
        // 1. Destrói os modelos 3D que estão fisicamente dentro do carrinho
        if (cartManager != null)
        {
            foreach (GameObject item in cartManager.collectedProducts)
            {
                Destroy(item);
            }
            
            // 2. Zera a memória e a conta do carrinho
            cartManager.collectedProducts.Clear();
            cartManager.totalPrice = 0f;
        }

        // 3. Libera o jogador para continuar jogando
        hasCompletedCheckout = false; // Permite passar no caixa de novo na próxima compra
        CloseCheckout();
        cartManager = null; // Desconecta do carrinho atual
    }

    public void ContinueShopping()
    {
        CloseCheckout();
    }
}
