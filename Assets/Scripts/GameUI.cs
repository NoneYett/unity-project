using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class GameUI : MonoBehaviour
{
    [Header("HUD Elementos")]
    public TextMeshProUGUI totalPriceText;
    public TextMeshProUGUI interactionText;
    public GameObject crosshair;
    
    [Header("Lista de Compras")]
    public GameObject shoppingListPanel;
    public Transform shoppingListContainer;
    public GameObject shoppingListItemPrefab;
    
    [Header("Configurações")]
    public float interactionTextFadeDuration = 0.3f;

    private CanvasGroup interactionCanvasGroup;
    private Dictionary<string, TextMeshProUGUI> listItems = new Dictionary<string, TextMeshProUGUI>();

    public static GameUI Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (interactionText != null)
        {
            interactionCanvasGroup = interactionText.GetComponent<CanvasGroup>();
            if (interactionCanvasGroup == null)
                interactionCanvasGroup = interactionText.gameObject.AddComponent<CanvasGroup>();
            interactionCanvasGroup.alpha = 0;
        }

        HideInteractionText();
    }

    public void UpdateTotalPrice(float price)
    {
        if (totalPriceText != null)
        {
            totalPriceText.text = "R$ " + price.ToString("F2");
        }
    }

    public void ShowInteractionText(string text)
    {
        if (interactionText != null)
        {
            interactionText.text = text;
            if (interactionCanvasGroup != null)
                interactionCanvasGroup.alpha = 1;
        }
    }

    public void HideInteractionText()
    {
        if (interactionCanvasGroup != null)
        {
            interactionCanvasGroup.alpha = 0;
        }
    }

    public void AddToShoppingList(string itemName)
    {
        if (shoppingListContainer == null || shoppingListItemPrefab == null) return;
        if (listItems.ContainsKey(itemName)) return;

        GameObject newItem = Instantiate(shoppingListItemPrefab, shoppingListContainer);
        TextMeshProUGUI itemText = newItem.GetComponentInChildren<TextMeshProUGUI>();
        
        if (itemText != null)
        {
            itemText.text = "○ " + itemName;
            listItems[itemName] = itemText;
        }
    }

    public void MarkItemAsCollected(string itemName)
    {
        if (listItems.ContainsKey(itemName))
        {
            TextMeshProUGUI itemText = listItems[itemName];
            itemText.text = "✓ <s>" + itemName + "</s>";
            itemText.color = new Color(0.5f, 0.5f, 0.5f, 1f);
        }
    }

    public void UnmarkItem(string itemName)
    {
        if (listItems.ContainsKey(itemName))
        {
            TextMeshProUGUI itemText = listItems[itemName];
            itemText.text = "○ " + itemName;
            itemText.color = Color.white;
        }
    }

    public void ClearShoppingList()
    {
        foreach (var item in listItems.Values)
        {
            if (item != null)
                Destroy(item.transform.parent.gameObject);
        }
        listItems.Clear();
    }

    public void ShowCrosshair()
    {
        if (crosshair != null)
            crosshair.SetActive(true);
    }

    public void HideCrosshair()
    {
        if (crosshair != null)
            crosshair.SetActive(false);
    }
}
