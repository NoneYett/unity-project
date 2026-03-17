using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Referências")]
    public GameObject playerPrefab;
    public Transform playerSpawnPoint;
    public CartManager cartManager;
    
    [Header("Configurações de Jogo")]
    public int targetItems = 5;
    public float gameTimeLimit = 0f; // 0 = sem limite
    
    private float gameTimer = 0f;
    private bool isGameActive = true;

    public static GameManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Inicializa o jogo
        isGameActive = true;
        gameTimer = 0f;
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (!isGameActive) return;

        // Atualiza timer se houver limite
        if (gameTimeLimit > 0)
        {
            gameTimer += Time.deltaTime;
            
            if (gameTimer >= gameTimeLimit)
            {
                EndGame(false);
            }
        }
    }

    public void EndGame(bool victory)
    {
        isGameActive = false;
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        if (victory)
        {
            Debug.Log("Vitória! Compras finalizadas com sucesso.");
        }
        else
        {
            Debug.Log("Tempo esgotado!");
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public float GetGameTime()
    {
        return gameTimer;
    }

    public float GetTimeRemaining()
    {
        if (gameTimeLimit <= 0) return float.MaxValue;
        return Mathf.Max(0, gameTimeLimit - gameTimer);
    }

    public bool IsGameActive()
    {
        return isGameActive;
    }
}
