using UnityEngine;

public class BackupConfigurator : MonoBehaviour
{
    [Header("COMO USAR:")]
    [TextArea(3, 10)]
    public string instructions = "1. Clique com o botão DIREITO no nome deste script ali em cima.\n2. Escolha 'Executar Configurador Mágico'.\n\nSe não funcionar, verifique se há erros em VERMELHO na aba 'Console' lá embaixo.";

    [ContextMenu("Executar Configurador Mágico")]
    public void RunSetup()
    {
        Debug.Log("Iniciando configuração automática...");
        
        // Chamar os mesmos métodos que eu te ensinei
        // (Vou colocar a lógica simplificada aqui para garantir que funcione)
        
        // 1. Tags (via código simples)
        Debug.Log("Tags: Por favor, use o menu Tools se ele aparecer. Se não, este botão ajuda a configurar a escala.");

        // 2. Escala do Mercado
        GameObject store = GameObject.Find("Storepack");
        if (store != null)
        {
            store.transform.localScale = new Vector3(1f, 1f, 1f);
            Debug.Log("Mercado escalado!");
        }

        // 3. Produtos
        GameObject[] allObjects = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        foreach (GameObject go in allObjects)
        {
            string name = go.name.ToLower();
            if (name.Contains("apple") || name.Contains("bottle") || name.Contains("can_food"))
            {
                go.tag = "Produto";
            }
        }
        
        Debug.Log("Configuração de Backup Concluída!");
    }
}
