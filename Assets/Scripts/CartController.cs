using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CartController : MonoBehaviour
{
    public float speed = 5f;
    private Rigidbody rb;
    private Vector3 input = Vector3.zero;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // captura input em Update
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        input = new Vector3(h, 0f, v);

        // rota para direção de movimento (só visual)
        if (input.sqrMagnitude > 0.01f)
        {
            Quaternion target = Quaternion.LookRotation(new Vector3(input.x, 0, input.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, target, 10f * Time.deltaTime);
        }
    }

    void FixedUpdate()
    {
        // aplica movimento pelo Rigidbody
        Vector3 movement = input * speed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + movement);
    }
}
