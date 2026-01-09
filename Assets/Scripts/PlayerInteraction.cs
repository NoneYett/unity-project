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
                    cartDrivingPosition = currentCart.transform.Find("DrivingPosition");
                    GrabCart();
                }
            }
        }
    }

    void GrabCart()
    {
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
        playerController.enabled = true;
        currentCart.enabled = false;

        // "Desprende" o jogador do carrinho
        playerBody.SetParent(null);
    }
}