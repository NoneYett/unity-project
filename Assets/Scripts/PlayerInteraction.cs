using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Controles")]
    public float interactionDistance = 3f;
    public KeyCode interactionKey = KeyCode.E;

    [Header("Referências")]
    public Transform playerBody; // O corpo do seu jogador (o objeto pai da câmera)
    public CharacterController playerController; // O controlador de movimento do seu jogador

    private bool isPushingCart = false;
    private CartMovement currentCart;
    private Transform cartDrivingPosition;

    void Update()
    {
        if (isPushingCart)
        {
            // Se já está empurrando, a única ação possível é largar o carrinho
            if (Input.GetKeyDown(interactionKey))
            {
                ReleaseCart();
            }
        }
        else
        {
            // Se não está empurrando, verifica se está olhando para a alça do carrinho
            CheckForCart();
        }
    }

    void CheckForCart()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionDistance))
        {
            // Usamos uma Tag "CartHandle" para identificar a alça do carrinho
            if (hit.collider.CompareTag("CartHandle"))
            {
                // Mostra uma mensagem na tela (opcional)
                // UIManager.ShowInteractionText("Apertar E para empurrar");

                if (Input.GetKeyDown(interactionKey))
                {
                    // Pega as referências do carrinho que acertamos
                    currentCart = hit.collider.GetComponentInParent<CartMovement>();
                    if (currentCart == null)
                    {
                        Debug.LogWarning("Hit CartHandle but no CartMovement found in parent.");
                        return;
                    }

                    cartDrivingPosition = currentCart.transform.Find("DrivingPosition");
                    if (cartDrivingPosition == null)
                    {
                        Debug.LogWarning("CartDrivingPosition (child named 'DrivingPosition') not found on cart: " + currentCart.name);
                        return;
                    }

                    GrabCart();
                }
            }
        }
    }

    void GrabCart()
    {
        if (currentCart == null)
        {
            Debug.LogWarning("Attempted to grab cart but currentCart is null.");
            return;
        }

        if (playerController == null || playerBody == null)
        {
            Debug.LogWarning("PlayerController or PlayerBody reference is null on PlayerInteraction.");
            return;
        }

        isPushingCart = true;

        // Desativa o controle do jogador e ativa o do carrinho
        playerController.enabled = false;
        currentCart.enabled = true;

        // "Gruda" o jogador na posição de empurrar do carrinho
        playerBody.SetParent(cartDrivingPosition);
        playerBody.localPosition = Vector3.zero;
        playerBody.localRotation = Quaternion.identity;
    }

    void ReleaseCart()
    {
        isPushingCart = false;

        // Ativa o controle do jogador e desativa o do carrinho
        if (playerController != null) playerController.enabled = true;
        if (currentCart != null) currentCart.enabled = false;

        // "Desprende" o jogador do carrinho
        if (playerBody != null) playerBody.SetParent(null);
    }
}