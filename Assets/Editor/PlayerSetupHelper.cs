using UnityEngine;
using UnityEditor;

public class PlayerSetupHelper
{
    [MenuItem("Ferramentas/1. Criar e Consertar Jogador (Player)")]
    public static void SetupPlayer()
    {
        // 1. Destruir TODOS os players antigos para garantir instalação limpa
        FirstPersonPlayer[] existingPlayers = Object.FindObjectsByType<FirstPersonPlayer>(FindObjectsSortMode.None);
        foreach (FirstPersonPlayer player in existingPlayers)
        {
            Undo.DestroyObjectImmediate(player.gameObject);
        }

        Debug.Log("Criando um novo Player adaptado para a escala GIGANTE do supermercado...");

        // 2. Criar o objeto principal do jogador
        GameObject playerObj = new GameObject("Player_Scale8");
        playerObj.tag = "Player";
        
        // Colocando em um espaço livre entre prateleiras na cena original
        playerObj.transform.position = new Vector3(2f, 16f, 0f);
        
        // *** ESCALA GIGANTE ***
        // A cena tem assets com scale 8x8x8. O player padrão tem 2 de altura.
        // Vamos aplicar a escala 8x direto na raiz do player para que tudo acompanhe:
        playerObj.transform.localScale = new Vector3(8f, 8f, 8f);

        // 3. Configurar CharacterController
        CharacterController cc = playerObj.AddComponent<CharacterController>();
        // O CC usa unidades locais. Localmente, o player continua tendo 2 de altura.
        cc.height = 2f; 
        cc.radius = 0.5f;
        cc.center = new Vector3(0, 0, 0); 
        cc.stepOffset = 0.3f; // Proporcionalmente 8 * 0.3 = 2.4 metros de degrau

        // 4. Configurar script de movimentação
        FirstPersonPlayer fpp = playerObj.AddComponent<FirstPersonPlayer>();
        fpp.speed = 35f; // Aumentar MUITO a velocidade (7.5 * 5)
        fpp.gravity = -40f; // Aumentar MUITO a gravidade para não flutuar
        fpp.lookSpeed = 2f;
        fpp.interactionDistance = 40f; // Raio longo pro hitbox de itens grandalhões

        // 5. Configurar a Câmera
        Camera mainCam = Camera.main;
        GameObject camObj;
        
        if (mainCam != null)
        {
            camObj = mainCam.gameObject;
            camObj.transform.SetParent(playerObj.transform);
        }
        else
        {
            camObj = new GameObject("Main Camera");
            camObj.AddComponent<Camera>();
            camObj.AddComponent<AudioListener>();
            camObj.tag = "MainCamera";
            camObj.transform.SetParent(playerObj.transform);
        }

        // Posicionar a câmera ESPECIFICAMENTE na altura dos olhos proporcianal
        // Como o centro do CC é 0 e a altura é 2 (vai de -1 a 1),
        // O "olho" localmente fica em Y = 0.8
        camObj.transform.localPosition = new Vector3(0, 0.8f, 0);
        camObj.transform.localRotation = Quaternion.identity;
        fpp.playerCamera = camObj.GetComponent<Camera>();

        // EVITAR CORTES NA CÂMERA DEVIDO À ESCALA (Clipping planes)
        fpp.playerCamera.nearClipPlane = 1f; // Valores muito baixos em escalas gigantes bugam os polígonos
        fpp.playerCamera.farClipPlane = 2000f; // Poder ver longe

        // 6. Configurar o local de segurar produtos
        GameObject holdPos = new GameObject("HoldPosition");
        holdPos.transform.SetParent(camObj.transform);
        // Posição local da "mão"
        holdPos.transform.localPosition = new Vector3(0.5f, -0.4f, 1.2f); 
        holdPos.transform.localRotation = Quaternion.identity;
        
        // Se a holdPosition acompanhar a escala da câmera que tá na escala 8, os objetos 
        // ficam com tamanho real ao serem segurados, pois são "Parented" e absorvem a escala local.
        fpp.holdPosition = holdPos.transform;

        // 7. AUMENTAR O CHÃO ROSA
        ExpandFloor();

        // Registrar pro Ctrl+Z funcionar
        Undo.RegisterCreatedObjectUndo(playerObj, "Criar Player GIGANTE");
        
        // Selecionar o jogador criado
        Selection.activeGameObject = playerObj;

        Debug.Log("✅ Player GIGANTE criado com sucesso! Altura ajustada com a câmera a 16m do chão. Dê PLAY!");
    }

    private static void ExpandFloor()
    {
        // Ao invés de ficar adivinhando qual é o chão, vamos criar um Chao_Gigante novo e apagar o antigo
        // 1. Procurar chãos antigos ("Plane", "Floor") e esconder ou deletar
        MeshRenderer[] renderers = Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None);
        foreach (MeshRenderer mr in renderers)
        {
            Transform t = mr.transform;
            if (t.position.y < 5f && t.localScale.y <= 1f && t.localScale.x <= 15f)
            {
                if (mr.gameObject.name.ToLower().Contains("plane") || mr.gameObject.name.ToLower().Contains("floor"))
                {
                    Undo.DestroyObjectImmediate(mr.gameObject);
                }
            }
        }

        // 2. Criar um Chão novo gigantesco
        GameObject novoChao = GameObject.CreatePrimitive(PrimitiveType.Plane);
        novoChao.name = "Chao_Gigante";
        
        // Posição no centro da loja (Y=0, debaixo das prateleiras e caixas)
        novoChao.transform.position = new Vector3(100f, 0f, 60f);
        
        // Plane original tem 10x10 metros. Escala de 50 = 500x500 metros
        novoChao.transform.localScale = new Vector3(50f, 1f, 50f);

        // 3. Tentar achar o material rosa da cena original
        Material matRosa = null;
        MeshRenderer[] allRenderers = Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None);
        foreach (MeshRenderer mr in allRenderers)
        {
            if (mr.sharedMaterial != null && mr.sharedMaterial.name.ToLower().Contains("pink"))
            {
                matRosa = mr.sharedMaterial;
                break;
            }
        }

        // Aplicar material se achou, senão fica branco
        if (matRosa != null)
        {
            novoChao.GetComponent<MeshRenderer>().sharedMaterial = matRosa;
        }

        // Colisor já vem embutido no CreatePrimitive(Plane)
        Undo.RegisterCreatedObjectUndo(novoChao, "Criar Chão Gigante");
        Debug.Log("✅ Novo chão Gigante criado com sucesso cobrindo toda a área da cena!");
    }
}
