using UnityEngine;

public class InteractionHighlight : MonoBehaviour
{
    [Header("Configurações")]
    public Color highlightColor = new Color(1f, 0.9f, 0.5f, 1f);
    public float outlineWidth = 0.02f;
    
    private Material originalMaterial;
    private Material highlightMaterial;
    private Renderer objectRenderer;
    private bool isHighlighted = false;

    void Start()
    {
        objectRenderer = GetComponent<Renderer>();
        if (objectRenderer != null && objectRenderer.material != null)
        {
            originalMaterial = objectRenderer.material;
            CreateHighlightMaterial();
        }
    }

    void CreateHighlightMaterial()
    {
        // Cria uma cópia do material original com emissão aumentada
        highlightMaterial = new Material(originalMaterial);
        highlightMaterial.EnableKeyword("_EMISSION");
        highlightMaterial.SetColor("_EmissionColor", highlightColor * 0.3f);
    }

    public void EnableHighlight()
    {
        if (objectRenderer != null && highlightMaterial != null && !isHighlighted)
        {
            objectRenderer.material = highlightMaterial;
            isHighlighted = true;
        }
    }

    public void DisableHighlight()
    {
        if (objectRenderer != null && originalMaterial != null && isHighlighted)
        {
            objectRenderer.material = originalMaterial;
            isHighlighted = false;
        }
    }

    void OnDestroy()
    {
        if (highlightMaterial != null)
        {
            Destroy(highlightMaterial);
        }
    }
}
