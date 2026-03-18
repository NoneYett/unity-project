using UnityEngine;
using UnityEngine.Events;

public class PickupEffect : MonoBehaviour
{
    [Header("Configurações de Efeito")]
    public float scaleInDuration = 0.2f;
    public float floatAmplitude = 0.1f;
    public float floatSpeed = 2f;
    public bool enableFloating = true;
    public bool enableRotation = false;
    public float rotationSpeed = 50f;

    [Header("Eventos")]
    public UnityEvent onPickup;
    public UnityEvent onDrop;

    private Vector3 originalScale;
    private Vector3 startPosition;
    private bool isFloating = true;

    void Start()
    {
        originalScale = transform.localScale;
        startPosition = transform.position;
    }

    void Update()
    {
        if (isFloating && enableFloating)
        {
            // Efeito de flutuação suave
            float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }

        if (enableRotation)
        {
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        }
    }

public void OnPickedUp()
{
    isFloating = false;
    // Garante que a escala local seja 1 no momento em que entra na mão
    transform.localScale = originalScale;
    onPickup?.Invoke();
}

    public void OnDropped()
    {
        startPosition = transform.position;
        isFloating = true;
        onDrop?.Invoke();
    }

    // Animação de escala (pode ser chamada via código)
    public void PlayPickupAnimation()
    {
        StartCoroutine(ScaleAnimation());
    }

    System.Collections.IEnumerator ScaleAnimation()
    {
        Vector3 targetScale = originalScale * 1.2f;
        float elapsed = 0;
        
        // Scale up
        while (elapsed < scaleInDuration / 2)
        {
            elapsed += Time.deltaTime;
            transform.localScale = Vector3.Lerp(originalScale, targetScale, elapsed / (scaleInDuration / 2));
            yield return null;
        }

        // Scale down
        elapsed = 0;
        while (elapsed < scaleInDuration / 2)
        {
            elapsed += Time.deltaTime;
            transform.localScale = Vector3.Lerp(targetScale, originalScale, elapsed / (scaleInDuration / 2));
            yield return null;
        }

        transform.localScale = originalScale;
    }
}
