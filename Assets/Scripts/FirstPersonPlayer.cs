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
    public float interactionDistance = 5f; // Mantivemos a distância maior
    public KeyCode interactionKey = KeyCode.E;
    
    [Header("Sistema de Carrinho")]
    private bool isPushingCart = false;
    private CartMovement currentCart;
    private Transform cartDrivingPosition;

    [Header("Sistema de Segurar Produtos")]
    public Transform holdPosition;
    private Product heldProduct;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
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
        // DEBUG #1: Esta mensagem deve aparecer em TODOS os frames.
        Debug.Log("--- FRAME " + Time.frameCount + " ---");

        // DEBUG #2: Vamos verificar os estados principais
        if (isPushingCart)
        {
            Debug.Log("Estado Atual: Empurrando o Carrinho.");
        }
        else
        {
            Debug.Log("Estado Atual: Andando Livremente.");
        }

        if (heldProduct != null)
        {
            Debug.Log("Estado da Mão: Segurando o objeto '" + heldProduct.name + "'");
        }
        else
        {
            Debug.Log("Estado da Mão: Mãos Vazias.");
        }


        // Lógica Principal (com logs internos)
        if (isPushingCart)
        {
            if (Input.GetKeyDown(interactionKey))
            {
                Debug.Log("Ação: Tentando soltar o carrinho...");
                ReleaseCart();
            }
        }
        else // Se não estamos empurrando o carrinho...
        {
            if (heldProduct != null)
            {
                if (Input.GetKeyDown(interactionKey))
                {
                    Debug.Log("Ação: Tentando colocar/soltar o item '" + heldProduct.name + "'...");
                    
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
                                Debug.Log("--> Sucesso! Colocando item no carrinho.");
                                cartManager.AddProductToCart(heldProduct.gameObject);
                                heldProduct = null; 
                                return;
                            }
                        }
                    }
                    
                    Debug.Log("--> Alvo não era o carrinho. Soltando item no chão.");
                    DropProduct();
                }
            }
            else // Se não estamos segurando nada...
            {
                // DEBUG #3: Se o estado estiver correto, esta mensagem DEVE aparecer.
                Debug.Log("Ação: Procurando por itens para interagir...");
                CheckForInteractables();
            }
        }
    }

    void CheckForInteractables()
    {
        int layerMask = ~LayerMask.GetMask("Player");
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionDistance, layerMask))
        {
            if (hit.collider.CompareTag("CartHandle"))
            {
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
                    if (product.isCollected)
                    {
                        // DEBUG 1: Estamos detectando um produto já coletado?
                        Debug.Log("Produto COLETADO detectado: " + product.name);

                        if (Input.GetKeyDown(interactionKey))
                        {
                            // DEBUG 2: A tecla de interação foi pressionada?
                            Debug.Log("Tecla 'E' pressionada para remover o item.");

                            CartManager cart = hit.collider.GetComponentInParent<CartManager>();
                            if (cart == null)
                            {
                                // DEBUG 3: Se chegarmos aqui, este é o erro.
                                Debug.LogError("FALHA CRÍTICA: Não foi possível encontrar o 'CartManager' subindo a partir do produto! Verifique a hierarquia.");
                            }
                            else
                            {
                                // DEBUG 4: Se chegarmos aqui, tudo deveria funcionar.
                                Debug.Log("SUCESSO: CartManager encontrado! Removendo produto...");
                                cart.RemoveProductFromCart(product.gameObject);
                                PickupProduct(product);
                            }
                        }
                    }
                    else
                    {
                        if (Input.GetKeyDown(interactionKey))
                        {
                            PickupProduct(product);
                        }
                    }
                }
            }
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
        Rigidbody rb = heldProduct.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = false;
        heldProduct.GetComponent<Collider>().isTrigger = true;
        heldProduct.transform.SetParent(null);
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
    }
}