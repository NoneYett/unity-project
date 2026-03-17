using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class ShoppingListItem
{
    public string productName;
    public bool isCollected;
    public int quantity;
}

public class ShoppingList : MonoBehaviour
{
    [Header("Configurações")]
    public int numberOfItemsToGenerate = 5;
    public List<string> availableProducts = new List<string>
    {
        "Maçã Verde",
        "Água",
        "Garrafa Rosa",
        "Garrafa Azul",
        "Garrafa Verde",
        "Garrafa Vermelha",
        "Iogurte Morango",
        "Lata Azul",
        "Lata Marrom",
        "Lata Vermelha",
        "Chocolate",
        "Creme Amarelo",
        "Donut Vermelho",
        "Donut Branco",
        "Maionese",
        "Café",
        "Geleia Preta",
        "Geleia Roxa",
        "Batata",
        "Papel Higiênico",
        "Melancia"
    };

    private List<ShoppingListItem> currentList = new List<ShoppingListItem>();
    private int collectedCount = 0;

    public static ShoppingList Instance { get; private set; }

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
        GenerateNewList();
    }

    public void GenerateNewList()
    {
        currentList.Clear();
        collectedCount = 0;

        List<string> shuffledProducts = new List<string>(availableProducts);
        ShuffleList(shuffledProducts);

        int count = Mathf.Min(numberOfItemsToGenerate, shuffledProducts.Count);
        for (int i = 0; i < count; i++)
        {
            ShoppingListItem item = new ShoppingListItem
            {
                productName = shuffledProducts[i],
                isCollected = false,
                quantity = 1
            };
            currentList.Add(item);

            // Atualiza a UI
            if (GameUI.Instance != null)
            {
                GameUI.Instance.AddToShoppingList(item.productName);
            }
        }
    }

    void ShuffleList<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            T temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }

    public bool IsItemOnList(string productName)
    {
        foreach (var item in currentList)
        {
            if (item.productName.ToLower().Contains(productName.ToLower()) ||
                productName.ToLower().Contains(item.productName.ToLower()))
            {
                return true;
            }
        }
        return false;
    }

    public void MarkItemCollected(string productName)
    {
        foreach (var item in currentList)
        {
            if (!item.isCollected && 
                (item.productName.ToLower().Contains(productName.ToLower()) ||
                 productName.ToLower().Contains(item.productName.ToLower())))
            {
                item.isCollected = true;
                collectedCount++;

                if (GameUI.Instance != null)
                {
                    GameUI.Instance.MarkItemAsCollected(item.productName);
                }

                CheckCompletion();
                return;
            }
        }
    }

    public void UnmarkItem(string productName)
    {
        foreach (var item in currentList)
        {
            if (item.isCollected && 
                (item.productName.ToLower().Contains(productName.ToLower()) ||
                 productName.ToLower().Contains(item.productName.ToLower())))
            {
                item.isCollected = false;
                collectedCount--;

                if (GameUI.Instance != null)
                {
                    GameUI.Instance.UnmarkItem(item.productName);
                }
                return;
            }
        }
    }

    void CheckCompletion()
    {
        if (collectedCount >= currentList.Count)
        {
            Debug.Log("Lista de compras completa! Vá ao caixa para finalizar.");
        }
    }

    public int GetTotalItems()
    {
        return currentList.Count;
    }

    public int GetCollectedCount()
    {
        return collectedCount;
    }

    public float GetCompletionPercentage()
    {
        if (currentList.Count == 0) return 0f;
        return (float)collectedCount / currentList.Count * 100f;
    }

    public List<ShoppingListItem> GetCurrentList()
    {
        return currentList;
    }
}
