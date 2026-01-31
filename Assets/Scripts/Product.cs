using UnityEngine;

public enum ProductCategory
{
    Frutas,
    Bebidas,
    Laticinios,
    Enlatados,
    Doces,
    Higiene,
    Outros
}

public class Product : MonoBehaviour
{
    [Header("Informações do Produto")]
    public string productName = "Produto";
    public float price = 1.00f;
    public ProductCategory category = ProductCategory.Outros;
    
    [Header("Descrição")]
    [TextArea(2, 4)]
    public string description = "";

    [HideInInspector] public bool isCollected = false;

    private InteractionHighlight highlight;

    void Start()
    {
        highlight = GetComponent<InteractionHighlight>();
        if (highlight == null)
        {
            highlight = gameObject.AddComponent<InteractionHighlight>();
        }
    }

    public void OnLookAt()
    {
        if (highlight != null)
            highlight.EnableHighlight();
    }

    public void OnLookAway()
    {
        if (highlight != null)
            highlight.DisableHighlight();
    }

    public string GetDisplayName()
    {
        return productName;
    }

    public string GetPriceString()
    {
        return "R$ " + price.ToString("F2");
    }

    public string GetInteractionText()
    {
        if (isCollected)
            return "[E] Retirar do Carrinho";
        else
            return "[E] Pegar " + productName + " - " + GetPriceString();
    }
}
