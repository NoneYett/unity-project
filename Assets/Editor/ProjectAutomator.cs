#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;

public class ProjectAutomator : EditorWindow
{
    [MenuItem("Tools/Configurar Projeto Supermercado")]
    public static void ShowWindow() => GetWindow<ProjectAutomator>("Configurador");

    void OnGUI()
    {
        GUILayout.Label("🏪 Configurador de Supermercado", EditorStyles.boldLabel);
        
        EditorGUILayout.Space(10);
        GUILayout.Label("Layout", EditorStyles.miniBoldLabel);
        
        if (GUILayout.Button("🏗️ REORGANIZAR TUDO", GUILayout.Height(30))) 
            ReorganizarTudo();
        
        EditorGUILayout.Space(10);
        GUILayout.Label("Produtos", EditorStyles.miniBoldLabel);
        
        if (GUILayout.Button("🍎 MAÇÃS nas BANCAS", GUILayout.Height(25))) 
            CriarPiramide("DisplayShelf1", "DisplayShelf2");
        
        if (GUILayout.Button("🍾 PRODUTOS nas PRATELEIRAS (Raycast Cascata)", GUILayout.Height(25))) 
            ColocarProdutosNasPrateleiras();
        
        if (GUILayout.Button("📦 CAIXAS atrás do CAIXA", GUILayout.Height(25))) 
            PosicionarCaixas();
        
        if (GUILayout.Button("🗑️ LIMPAR PRODUTOS", GUILayout.Height(20))) 
            LimparProdutos();
    }

    // ==================== POSICIONAR CAIXAS ATRÁS DO CAIXA ====================
    static void PosicionarCaixas()
    {
        GameObject storepack = GameObject.Find("Storepack");
        if (storepack == null) {
            EditorUtility.DisplayDialog("Erro", "Storepack não encontrado!", "OK");
            return;
        }

        // Encontra o Counter para saber onde posicionar as caixas
        Transform counter = storepack.transform.Find("Counter");
        if (counter == null) {
            EditorUtility.DisplayDialog("Erro", "Counter não encontrado!", "OK");
            return;
        }

        // Procura objetos que parecem ser caixas (lista todos primeiro para debug)
        Debug.Log("=== OBJETOS NO STOREPACK ===");
        foreach (Transform child in storepack.transform) {
            Debug.Log($"  - {child.name}");
        }
        
        List<Transform> caixas = new List<Transform>();
        foreach (Transform child in storepack.transform) {
            string nome = child.name.ToLower();
            // Procura por vários nomes possíveis
            if (nome.Contains("cardboard") || nome.Contains("carton") || 
                nome.Contains("box") || nome.Contains("crate") || 
                nome.Contains("package") || nome.Contains("caixa")) {
                caixas.Add(child);
                Debug.Log($"CAIXA ENCONTRADA: {child.name}");
            }
        }

        if (caixas.Count == 0) {
            EditorUtility.DisplayDialog("Aviso", 
                "Nenhuma caixa encontrada no Storepack.\n" +
                "Procurando por: cardboard, carton, box", "OK");
            return;
        }

        // Posiciona as caixas atrás do Counter (Z mais negativo)
        float zAtrasDoCounter = counter.localPosition.z - 0.05f;
        float xInicio = counter.localPosition.x - 0.03f;
        
        int i = 0;
        foreach (Transform caixa in caixas) {
            caixa.localPosition = new Vector3(
                xInicio + (i * 0.02f),  // Lado a lado
                0,
                zAtrasDoCounter
            );
            caixa.localRotation = Quaternion.Euler(0, Random.Range(-10, 10), 0);
            i++;
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorUtility.DisplayDialog("Caixas Posicionadas!", 
            $"{caixas.Count} caixas atrás do caixa!", "OK");
    }

    // ==================== REORGANIZAR TUDO ====================
    static void ReorganizarTudo()
    {
        GameObject storepack = GameObject.Find("Storepack");
        if (storepack == null) {
            EditorUtility.DisplayDialog("Erro", "Storepack não encontrado!", "OK");
            return;
        }

        storepack.transform.position = Vector3.zero;
        storepack.transform.rotation = Quaternion.identity;
        storepack.transform.localScale = Vector3.one * 100f;

        Transform counter = storepack.transform.Find("Counter");
        Transform register = storepack.transform.Find("Register");
        Transform displayShelf1 = storepack.transform.Find("DisplayShelf1");
        Transform displayShelf2 = storepack.transform.Find("DisplayShelf2");
        Transform shelf = storepack.transform.Find("Shelf");
        Transform shelf2 = storepack.transform.Find("Shelf2");
        Transform rack = storepack.transform.Find("Rack");
        Transform fridge = storepack.transform.Find("Fridge");

        float xEsq = -0.05f;
        float xDir = 0.05f;

        if (displayShelf1 != null) {
            displayShelf1.localPosition = new Vector3(xEsq, 0, 0.12f);
            displayShelf1.localRotation = Quaternion.identity;
        }
        if (displayShelf2 != null) {
            displayShelf2.localPosition = new Vector3(xDir, 0, 0.12f);
            displayShelf2.localRotation = Quaternion.identity;
        }

        if (shelf != null) {
            shelf.localPosition = new Vector3(xEsq - 0.02f, 0, 0.04f);
            shelf.localRotation = Quaternion.Euler(0, 90, 0);
        }
        if (shelf2 != null) {
            shelf2.localPosition = new Vector3(xDir + 0.02f, 0, 0.04f);
            shelf2.localRotation = Quaternion.Euler(0, -90, 0);
        }

        if (rack != null) {
            rack.localPosition = new Vector3(xEsq - 0.02f, 0, -0.04f);
            rack.localRotation = Quaternion.Euler(0, 90, 0);
        }
        if (fridge != null) {
            fridge.localPosition = new Vector3(xDir + 0.02f, 0, -0.04f);
            fridge.localRotation = Quaternion.Euler(0, -90, 0);
        }

        if (counter != null) {
            counter.localPosition = new Vector3(0, 0, -0.10f);
            counter.localRotation = Quaternion.Euler(0, 0, 0);
        }

        if (register != null && counter != null) {
            register.localPosition = new Vector3(
                counter.localPosition.x + 0.02f,
                0,
                counter.localPosition.z
            );
            register.localRotation = Quaternion.Euler(0, 0, 0);
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorUtility.DisplayDialog("Layout Reorganizado!", 
            "✅ Bancas na ENTRADA\n✅ Prateleiras no CORREDOR\n✅ Caixa na SAÍDA", "OK");
    }

    // ==================== PRODUTOS NAS PRATELEIRAS (RAYCAST CASCATA) ====================
    static void ColocarProdutosNasPrateleiras()
    {
        LimparProdutos();

        GameObject storepack = GameObject.Find("Storepack");
        if (storepack == null) return;

        // Garante colliders no móvel
        foreach (Transform child in storepack.transform) {
            foreach (var mf in child.GetComponentsInChildren<MeshFilter>()) {
                if (mf.gameObject.GetComponent<Collider>() == null) {
                    mf.gameObject.AddComponent<MeshCollider>();
                }
            }
        }
        Physics.SyncTransforms();

        // Encontra produtos na cena
        List<GameObject> produtos = new List<GameObject>();
        foreach (var obj in Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None)) {
            string nome = obj.name.ToLower();
            if ((nome.Contains("bottle") || nome.Contains("can") || nome.Contains("jar") || 
                 nome.Contains("chocolate") || nome.Contains("cream")) && 
                !nome.Contains("_shelf") && !nome.Contains("_pyramid")) {
                produtos.Add(obj);
            }
        }

        if (produtos.Count == 0) {
            EditorUtility.DisplayDialog("Erro", "Nenhum produto encontrado!", "OK");
            return;
        }

        Transform fridge = storepack.transform.Find("Fridge");
        Transform shelf = storepack.transform.Find("Shelf");
        Transform shelf2 = storepack.transform.Find("Shelf2");
        Transform rack = storepack.transform.Find("Rack");
        int totalProdutos = 0;

        // Preenche TODAS as prateleiras
        if (fridge != null) totalProdutos += PreencherTodasPrateleiras(fridge, produtos);
        if (shelf != null) totalProdutos += PreencherTodasPrateleiras(shelf, produtos);
        if (shelf2 != null) totalProdutos += PreencherTodasPrateleiras(shelf2, produtos);
        if (rack != null) totalProdutos += PreencherTodasPrateleiras(rack, produtos);

        // Desativa colliders dos produtos após colocar
        foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None)) {
            if (go.name.Contains("_shelf")) {
                Collider col = go.GetComponent<Collider>();
                if (col != null) col.enabled = false;
            }
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorUtility.DisplayDialog("Produtos Colocados!", 
            $"{totalProdutos} produtos distribuídos nas prateleiras!", "OK");
    }

    static int PreencherTodasPrateleiras(Transform movel, List<GameObject> produtos)
    {
        Renderer rnd = movel.GetComponentInChildren<Renderer>();
        if (rnd == null) return 0;

        Bounds bounds = rnd.bounds;
        float minX = bounds.min.x;
        float maxX = bounds.max.x;
        float minZ = bounds.min.z;
        float maxZ = bounds.max.z;
        float topoY = bounds.max.y;
        float baseY = bounds.min.y;

        float escala = 8f;
        int cols = 10;  // Mais produtos por linha
        int rows = 3;   // Mais linhas por prateleira

        float espacoX = (maxX - minX) / (cols + 1);
        float espacoZ = (maxZ - minZ) / (rows + 1);

        int count = 0;
        int numPrateleiras = 10;  // Muitos níveis para garantir TODAS as prateleiras

        // Para cada prateleira (de cima para baixo)
        for (int prateleira = 0; prateleira < numPrateleiras; prateleira++)
        {
            // Altura inicial do raycast para esta prateleira
            // Começa do topo e vai descendo
            float alturaRaycast = topoY - (prateleira * (topoY - baseY) / numPrateleiras) + 10f;
            
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    float x = minX + (c + 1) * espacoX;
                    float z = minZ + (r + 1) * espacoZ;

                    // Raycast de cima para baixo
                    Vector3 rayOrigin = new Vector3(x, alturaRaycast, z);
                    
                    if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, 500f))
                    {
                        // Só coloca se:
                        // 1. Acertou algo acima da base
                        // 2. NÃO está no topo do móvel (pelo menos 5 unidades abaixo)
                        if (hit.point.y > baseY + 1f && hit.point.y < topoY - 5f)
                        {
                            GameObject prodOriginal = produtos[Random.Range(0, produtos.Count)];
                            
                            GameObject clone = Object.Instantiate(prodOriginal);
                            clone.name = prodOriginal.name + "_shelf";
                            clone.SetActive(true);
                            clone.transform.position = new Vector3(x, hit.point.y + 0.5f, z);
                            clone.transform.localScale = Vector3.one * escala;
                            clone.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);

                            // Adiciona collider para próximos raycasts
                            BoxCollider bc = clone.GetComponent<BoxCollider>();
                            if (bc == null) bc = clone.AddComponent<BoxCollider>();
                            bc.enabled = true;

                            count++;
                        }
                    }
                }
            }
            
            // Sincroniza física para próxima prateleira
            Physics.SyncTransforms();
        }

        return count;
    }

    // ==================== PIRÂMIDE DE MAÇÃS ====================
    static void CriarPiramide(string movel1, string movel2)
    {
        foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None)) {
            if (go != null && go.name.Contains("_pyramid")) Object.DestroyImmediate(go);
        }

        GameObject storepack = GameObject.Find("Storepack");
        if (storepack == null) return;

        foreach (Transform child in storepack.transform) {
            foreach (var mf in child.GetComponentsInChildren<MeshFilter>()) {
                if (mf.gameObject.GetComponent<Collider>() == null) {
                    mf.gameObject.AddComponent<MeshCollider>();
                }
            }
        }
        Physics.SyncTransforms();

        GameObject macaOriginal = null;
        foreach (var obj in Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None)) {
            if (obj.name.ToLower().Contains("apple") && !obj.name.Contains("_pyramid")) {
                macaOriginal = obj;
                break;
            }
        }

        if (macaOriginal == null) {
            EditorUtility.DisplayDialog("Erro", "Nenhuma maçã encontrada!", "OK");
            return;
        }

        int totalMacas = 0;

        Transform t1 = storepack.transform.Find(movel1);
        if (t1 != null) totalMacas += MontarPiramide(t1, macaOriginal);

        if (movel2 != null) {
            Transform t2 = storepack.transform.Find(movel2);
            if (t2 != null) totalMacas += MontarPiramide(t2, macaOriginal);
        }

        foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None)) {
            if (go.name.Contains("_pyramid")) {
                Collider col = go.GetComponent<Collider>();
                if (col != null) col.enabled = false;
            }
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorUtility.DisplayDialog("Pirâmide Criada!", $"{totalMacas} maçãs!", "OK");
    }

    static int MontarPiramide(Transform movel, GameObject macaOriginal)
    {
        Renderer rnd = movel.GetComponentInChildren<Renderer>();
        if (rnd == null) return 0;

        Bounds bounds = rnd.bounds;
        float centroX = bounds.center.x;
        float centroZ = bounds.center.z;
        float largura = bounds.size.x;
        float profundidade = bounds.size.z;
        float topoY = bounds.max.y;

        float escalaMaca = 9f;
        float espacoX = largura / 9f;
        float espacoZ = profundidade / 7f;
        float alturaRaycast = topoY + 100f;

        int[][] camadas = new int[][] {
            new int[] { 8, 6 },
            new int[] { 7, 5 },
            new int[] { 6, 4 },
            new int[] { 5, 3 },
            new int[] { 4, 2 },
            new int[] { 3, 1 },
        };

        int count = 0;

        for (int camadaIdx = 0; camadaIdx < camadas.Length; camadaIdx++)
        {
            int cols = camadas[camadaIdx][0];
            int lins = camadas[camadaIdx][1];

            float larguraCamada = (cols - 1) * espacoX;
            float profundidadeCamada = (lins - 1) * espacoZ;
            float inicioX = centroX - (larguraCamada / 2);
            float inicioZ = centroZ - (profundidadeCamada / 2);

            for (int l = 0; l < lins; l++)
            {
                for (int c = 0; c < cols; c++)
                {
                    float x = inicioX + c * espacoX;
                    float z = inicioZ + l * espacoZ;

                    Vector3 rayOrigin = new Vector3(x, alturaRaycast, z);
                    float yFinal = topoY + 0.3f;

                    if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, 200f))
                    {
                        yFinal = hit.point.y + 0.4f;
                    }

                    GameObject clone = Object.Instantiate(macaOriginal);
                    clone.name = macaOriginal.name + "_pyramid";
                    clone.SetActive(true);
                    clone.transform.position = new Vector3(x, yFinal, z);
                    clone.transform.localScale = Vector3.one * escalaMaca;
                    clone.transform.rotation = Quaternion.Euler(
                        Random.Range(-5, 5), 
                        Random.Range(0, 360), 
                        Random.Range(-5, 5)
                    );

                    SphereCollider sc = clone.GetComponent<SphereCollider>();
                    if (sc == null) sc = clone.AddComponent<SphereCollider>();
                    sc.enabled = true;
                    sc.radius = 0.05f;

                    count++;
                }
            }

            Physics.SyncTransforms();
        }

        return count;
    }

    static void LimparProdutos()
    {
        foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None)) {
            if (go != null && (go.name.Contains("_pyramid") || go.name.Contains("_shelf"))) {
                Object.DestroyImmediate(go);
            }
        }
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
    }
}
#endif
