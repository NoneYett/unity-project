using UnityEngine;

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
        if (isPushingCart)
        {
            // Esconde texto de interação enquanto empurra carrinho
            if (GameUI.Instance != null)
                GameUI.Instance.ShowInteractionText("[E] Soltar Carrinho");

            if (Input.GetKeyDown(interactionKey))
            {
                ReleaseCart();
            }
        }
        else
        {
            if (heldProduct != null)
            {
                if (GameUI.Instance != null)
                    GameUI.Instance.ShowInteractionText("[E] Soltar " + heldProduct.productName);

                if (Input.GetKeyDown(interactionKey))
                {
                    int layerMask = ~LayerMask.GetMask("Player");
                    Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
                    RaycastHit hit;
                    
                    if (Physics.Raycast(ray, out hit, interactionDistance, layerMask))
                    {
                        if (hit.collider.CompareTag("ShoppingCart"))
                        {
                            CartManager cartManager = hit.collider.GetComponent<CartManager>();
                            if (cartManager != null)
                            {
                                cartManager.AddProductToCart(heldProduct.gameObject);
                                
                                // Marca na lista de compras
                                if (ShoppingList.Instance != null)
                                    ShoppingList.Instance.MarkItemCollected(heldProduct.productName);
                                
                                // Toca som
                                if (AudioManager.Instance != null)
                                    AudioManager.Instance.PlayCartAdd();
                                
                                heldProduct = null;
                                return;
                            }
                        }
                    }
                    
                    DropProduct();
                }
            }
            else
            {
                CheckForInteractables();
            }
        }
    }

    void CheckForInteractables()
    {
        int layerMask = ~LayerMask.GetMask("Player");
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        // Limpa highlight do objeto anterior
        if (lastLookedProduct != null)
        {
            lastLookedProduct.OnLookAway();
            lastLookedProduct = null;
        }

        if (Physics.Raycast(ray, out hit, interactionDistance, layerMask))
        {
            if (hit.collider.CompareTag("CartHandle"))
            {
                if (GameUI.Instance != null)
                    GameUI.Instance.ShowInteractionText("[E] Empurrar Carrinho");

                if (Input.GetKeyDown(interactionKey))
                {
                    currentCart = hit.collider.GetComponentInParent<CartMovement>();
                    cartDrivingPosition = currentCart.drivingPosition;
                    GrabCart();
                }
            }
            else if (hit.collider.CompareTag("Produto"))
            {
                Product product = hit.collider.GetComponent<Product>();
                if (product != null)
                {
                    // Ativa highlight
                    product.OnLookAt();
                    lastLookedProduct = product;

                    // Mostra texto de interação
                    if (GameUI.Instance != null)
                        GameUI.Instance.ShowInteractionText(product.GetInteractionText());

                    if (product.isCollected)
                    {
                        if (Input.GetKeyDown(interactionKey))
                        {
                            CartManager cart = hit.collider.GetComponentInParent<CartManager>();
                            if (cart != null)
                            {
                                cart.RemoveProductFromCart(product.gameObject);
                                
                                // Desmarca da lista de compras
                                if (ShoppingList.Instance != null)
                                    ShoppingList.Instance.UnmarkItem(product.productName);
                                
                                // Toca som
                                if (AudioManager.Instance != null)
                                    AudioManager.Instance.PlayCartRemove();
                                
                                PickupProduct(product);
                            }
                        }
                    }
                    else
                    {
                        if (Input.GetKeyDown(interactionKey))
                        {
                            // Toca som
                            if (AudioManager.Instance != null)
                                AudioManager.Instance.PlayPickup();
                            
                            PickupProduct(product);
                        }
                    }
                }
            }
            else
            {
                // Não está olhando para nada interativo
                if (GameUI.Instance != null)
                    GameUI.Instance.HideInteractionText();
            }
        }
        else
        {
            // Não está olhando para nada
            if (GameUI.Instance != null)
                GameUI.Instance.HideInteractionText();
        }
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
    }

    void DropProduct()
    {
        if (heldProduct == null) return;
        
        Rigidbody rb = heldProduct.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = false;
        heldProduct.GetComponent<Collider>().isTrigger = false;
        heldProduct.transform.SetParent(null);
        
        // Toca som
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayDrop();
        
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