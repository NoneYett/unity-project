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

    private bool isCheckoutActive = false;
    private bool hasCompletedCheckout = false;

    void Start()
    {
        if (checkoutPanel != null)
            checkoutPanel.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("ShoppingCart") && !hasCompletedCheckout)
        {
            OpenCheckout();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("ShoppingCart") && !hasCompletedCheckout)
        {
            CloseCheckout();
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

        // O CartManager precisará expor esses dados
        // Por enquanto usamos reflexão ou métodos públicos que adicionaremos

        float total = GetCartTotal();
        int itemCount = GetCartItemCount();

        if (totalText != null)
            totalText.text = "Total: R$ " + total.ToString("F2");

        if (itemsCountText != null)
            itemsCountText.text = itemCount + " itens no carrinho";

        // Calcular pontuação baseada em lista de compras
        int score = CalculateScore();
        if (scoreText != null)
            scoreText.text = "Pontuação: " + score + " pontos";
    }

    float GetCartTotal()
    {
        if (cartManager != null)
            return cartManager.GetTotalPrice();
        return 0f;
    }

    int GetCartItemCount()
    {
        if (cartManager != null)
            return cartManager.GetItemCount();
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
        hasCompletedCheckout = true;
        Time.timeScale = 1f;
        
        // Mostrar tela de vitória ou voltar ao menu
        SceneManager.LoadScene("MainMenu");
    }

    public void ContinueShopping()
    {
        CloseCheckout();
    }
}
