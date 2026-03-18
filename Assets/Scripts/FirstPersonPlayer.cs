using UnityEngine;
using TMPro;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonPlayer : MonoBehaviour
{
    [Header("Movimento do Jogador")]
    public float speed = 7.5f;
    public float gravity = -9.81f;
    private Vector3 velocity;
    private CharacterController controller;

    [Header("Câmera do Jogador")]
    public Camera playerCamera;
    public float lookSpeed = 2.0f;
    public float lookXLimit = 90.0f;
    private float rotationX = 0;
    public TextMeshProUGUI textoAviso; // O campo que vai aparecer no Inspector

    [Header("Interação")]
    public float interactionDistance = 5f;
    public KeyCode interactionKey = KeyCode.E;
    
    [Header("Sistema de Carrinho")]
    private bool isPushingCart = false;
    private CartMovement currentCart;
    private Transform cartDrivingPosition;

    [Header("Sistema de Segurar Produtos")]
    public Transform holdPosition;
    private Product heldProduct;

    // Referência ao último objeto olhado para highlight
    private Product lastLookedProduct;
    private PauseMenu pauseMenu;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        pauseMenu = Object.FindFirstObjectByType<PauseMenu>();
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Não processa input se estiver pausado
        if (pauseMenu != null && pauseMenu.IsPaused())
            return;

        rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
        rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
        transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);

        HandleMovement();
        HandleInteraction();
    }

    void HandleMovement()
    {
        if (isPushingCart) return;

        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");
        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        controller.Move(move * speed * Time.deltaTime);

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void HandleInteraction()
    {
        // 1. ESTADO: EMPURRANDO O CARRINHO
        if (isPushingCart)
        {
            if (textoAviso != null)
            {
                textoAviso.gameObject.SetActive(true);
                textoAviso.text = "[E] Soltar Carrinho";
            }
            
            if (Input.GetKeyDown(interactionKey)) ReleaseCart();
            return;
        }

        // 2. ESTADO: SEGURANDO UM PRODUTO (MÃO CHEIA)
        if (heldProduct != null)
        {
            int layerMask = ~LayerMask.GetMask("Player");
            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            
            // Atira o laser
            RaycastHit[] hits = Physics.RaycastAll(ray, interactionDistance, layerMask, QueryTriggerInteraction.Collide);
            
            bool lookingAtCart = false;
            CartManager foundCart = null;

            foreach (var hit in hits)
            {
                // Ignora o item na mão
                if (hit.collider.gameObject == heldProduct.gameObject || hit.collider.transform.IsChildOf(heldProduct.transform)) 
                    continue;

                // Verifica o carrinho (as paredes com a tag ShoppingCart)
                if (hit.collider.CompareTag("ShoppingCart"))
                {
                    lookingAtCart = true;
                    foundCart = hit.collider.transform.root.GetComponentInChildren<CartManager>();
                    break;
                }
            }

            // --- ATUALIZA O TEXTMESHPRO NA TELA ---
            if (textoAviso != null)
            {
                textoAviso.gameObject.SetActive(true); 

                if (lookingAtCart) 
                    textoAviso.text = "[E] Guardar no Carrinho";
                else 
                    textoAviso.text = "[E] Soltar " + heldProduct.productName;
            }

            // Executa a ação (Guardar ou Soltar no chão)
            if (Input.GetKeyDown(interactionKey))
            {
                if (lookingAtCart && foundCart != null) AddProductToCart(foundCart);
                else DropProduct();
            }
        }
        // 3. ESTADO: MÃO VAZIA
        else
        {
            CheckForInteractables(); 
        }
    }
void CheckForInteractables()
{
    int layerMask = ~LayerMask.GetMask("Player");
    Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
    
    // Pegamos TODOS os objetos que o laser atravessou
    RaycastHit[] hits = Physics.RaycastAll(ray, interactionDistance, layerMask);

    if (lastLookedProduct != null)
    {
        lastLookedProduct.OnLookAway();
        lastLookedProduct = null;
    }

    Product detectedProduct = null;
    GameObject detectedHandle = null;
    GameObject detectedCartBody = null;

    // 1. Varre os hits para encontrar o que é mais importante (o Produto)
    foreach (var hit in hits)
    {
        // --- CÓDIGO NOVO: IGNORA O ITEM NA MÃO ---
        if (heldProduct != null && hit.collider.gameObject == heldProduct.gameObject)
        {
            continue; // Pula para o próximo objeto que o laser atravessou
        }
        // -----------------------------------------

        Debug.Log("RAIO-X: Atravessei o objeto: " + hit.collider.name + " (Tag: " + hit.collider.tag + ")");

        if (hit.collider.CompareTag("Produto"))
        {
            detectedProduct = hit.collider.GetComponent<Product>();
            break; // Se achou um produto, ele é a prioridade absoluta
        }
        if (hit.collider.CompareTag("CartHandle")) detectedHandle = hit.collider.gameObject;
        if (hit.collider.CompareTag("ShoppingCart")) detectedCartBody = hit.collider.gameObject;
    }

    // 2. Executa a ação baseada no que foi encontrado
    if (detectedProduct != null)
    {
        ProcessProductInteraction(detectedProduct);
    }
    else if (detectedHandle != null)
    {
        if (GameUI.Instance != null) textoAviso.text = "[E] Empurrar Carrinho";
        if (Input.GetKeyDown(interactionKey)) {
            currentCart = detectedHandle.GetComponentInParent<CartMovement>();
            cartDrivingPosition = currentCart.drivingPosition;
            GrabCart();
        }
    }
    else if (detectedCartBody != null && heldProduct != null)
    {
        // Agora o código finalmente reconhece o Cart para você guardar o item!
        if (GameUI.Instance != null) textoAviso.text = "[E] Guardar no Carrinho";
        if (Input.GetKeyDown(interactionKey)) {
            CartManager cart = detectedCartBody.GetComponentInParent<CartManager>();
            if (cart != null) AddProductToCart(cart);
        }
    }
    else
    {
        if (GameUI.Instance != null) GameUI.Instance.HideInteractionText();
    }
}

// Função auxiliar para não bagunçar o CheckForInteractables
void ProcessProductInteraction(Product product)
{
    product.OnLookAt();
    lastLookedProduct = product;
    if (GameUI.Instance != null) GameUI.Instance.ShowInteractionText(product.GetInteractionText());

    if (Input.GetKeyDown(interactionKey))
    {
        if (product.isCollected)
        {
            CartManager cart = product.GetComponentInParent<CartManager>();
            if (cart != null)
            {
                cart.RemoveProductFromCart(product.gameObject);
                if (ShoppingList.Instance != null) ShoppingList.Instance.UnmarkItem(product.productName);
                if (AudioManager.Instance != null) AudioManager.Instance.PlayCartRemove();
                PickupProduct(product);
            }
        }
        else
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlayPickup();
            PickupProduct(product);
        }
    }
}
// ADICIONE ESTA FUNÇÃO PARA CORRIGIR O ERRO CS0103
void AddProductToCart(CartManager cart)
{
    if (heldProduct == null) return;
    cart.AddProductToCart(heldProduct.gameObject);
    if (ShoppingList.Instance != null) ShoppingList.Instance.MarkItemCollected(heldProduct.productName); // Nome exato do seu script
    if (AudioManager.Instance != null) AudioManager.Instance.PlayCartAdd();
    heldProduct = null;
}
    void PickupProduct(Product product)
    {
        if (product.isCollected) return;

        heldProduct = product;
        Rigidbody rb = heldProduct.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;
        heldProduct.GetComponent<Collider>().isTrigger = true;
        heldProduct.transform.SetParent(holdPosition);
        heldProduct.transform.localPosition = Vector3.zero;
        heldProduct.transform.localRotation = Quaternion.identity;

        // --- LINHA NOVA: Força a escala a ser sempre 9 (ou o valor que você quiser)
        //heldProduct.transform.localScale = transform.localScale;
    }

    void DropProduct()
    {
        if (heldProduct == null) return; // Segurança para não dar erro se a mão estiver vazia

        Rigidbody rb = heldProduct.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = false;

        // A MUDANÇA ESTÁ AQUI:
        // Mudamos para 'false' para que a maçã tenha colisão física com o chão
        heldProduct.GetComponent<Collider>().isTrigger = false; 
    
        heldProduct.transform.SetParent(null);
        //heldProduct = null;

        // Toca som
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayDrop();
        
        //heldProduct.transform.localScale = transform.localScale;

        heldProduct = null;
    }

    void GrabCart()
    {
        isPushingCart = true;
        currentCart.enabled = true;
        transform.SetParent(cartDrivingPosition);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        controller.enabled = false;
    }

    void ReleaseCart()
    {
        isPushingCart = false;
        currentCart.enabled = false;
        transform.SetParent(null);
        controller.enabled = true;
        
        if (GameUI.Instance != null)
            GameUI.Instance.HideInteractionText();
    }
}