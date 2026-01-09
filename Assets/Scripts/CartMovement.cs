using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CartMovement : MonoBehaviour
{
    public Transform drivingPosition;

    [Header("Configurações de Movimento")]
    public float speed = 4f;
    public float turnSpeed = 100f;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        // Pega o input para FRENTE/TRÁS (W/S)
        float moveInput = Input.GetAxis("Vertical");
        // Pega o input para GIRAR (A/D)
        float turnInput = Input.GetAxis("Horizontal");

        // --- LÓGICA DE MOVIMENTO PARA FRENTE ---
        Vector3 movement = transform.forward * moveInput * speed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + movement);

        // --- LÓGICA DE ROTAÇÃO PARA OS LADOS ---
        Quaternion turnRotation = Quaternion.Euler(0f, turnInput * turnSpeed * Time.fixedDeltaTime, 0f);
        rb.MoveRotation(rb.rotation * turnRotation);
    }
}