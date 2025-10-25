using UnityEngine;

/// @brief Gerencia os principais elementos e estados da simulação.
/// @details Controla a geração de comida, spawn de inimigos, inicialização da simulação e parâmetros de teste.
public class GameManager : MonoBehaviour
{
    /// @brief Tamanho total do mapa da simulação.
    public static Vector2 mapSize { get; private set; } = new Vector2(100.0f, 100.0f);

    [Header("Tests")]
    [SerializeField] private bool testingHunger;
    [SerializeField] private bool testingSleep;
    [SerializeField] private bool testingEnemy;
    [SerializeField] private bool preventEnemySpawn;
    private GameObject currentTestEnemy;

    [Header("Simulation")]
    [SerializeField] private float startingTimeScale;

    [Header("Food")]
    [SerializeField] private int foodPerAxis;
    [Tooltip("Min distance from wall for objects to spawn")]
    [SerializeField] private float wallDistance;
    [SerializeField] private float foodRandOffset;

    [Header("Enemies")]
    [SerializeField] private float timeBetweenEnemies = 5.0f;
    [SerializeField] private float enemySpawnPlusDist = 5.0f;
    private float enemySpawnTimer = 0.0f;

    [Header("Assignables")]
    [SerializeField] private AIStatus ai_status;
    [SerializeField] private GameObject foodPrefab;
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private Transform foodParent;

    /// @brief Instância global do GameManager.
    public static GameManager instance { get; private set; }

    /// @brief Inicializa a instância do GameManager e configura parâmetros iniciais.
    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this.gameObject);
            return;
        }
        else
            instance = this;

        enemySpawnTimer = timeBetweenEnemies;

        // Config simulation
        Application.targetFrameRate = 120;
    }

    /// @brief Configura a simulação no início da execução.
    private void Start()
    {
        if (!testingHunger)
            GenerateFood();

        if (testingSleep)
            ai_status.SetHungerPerSecond(0.0f);

        Time.timeScale = startingTimeScale;

        SimManager.instance.StartWatching();
    }

    /// @brief Atualiza o loop principal da simulação.
    /// @details Gerencia o tempo entre spawns de inimigos e permite spawn manual com a tecla E.
    private void Update()
    {
        if (!preventEnemySpawn)
        {
            enemySpawnTimer -= Time.deltaTime;
            if (enemySpawnTimer <= 0.0f)
                SpawnEnemy();
        }

        if (Input.GetKeyDown(KeyCode.E))
            SpawnEnemy(true);
    }

    /// @brief Gera as fontes de alimento distribuídas pelo mapa.
    private void GenerateFood()
    {
        Vector2 posToSpawn = Vector2.zero;
        Vector2 spawnArea = new Vector2(mapSize.x - wallDistance, mapSize.y - wallDistance);

        for (uint i = 0; i <= foodPerAxis; i++)
        {
            for (uint j = 0; j <= foodPerAxis; j++)
            {
                // Ignorar centro
                if (i == 5 && j == 5)
                    continue;

                posToSpawn.x = -spawnArea.x + ((spawnArea.x * 2 / foodPerAxis) * i);
                posToSpawn.x += Random.Range(-foodRandOffset, foodRandOffset);
                posToSpawn.y = -spawnArea.y + ((spawnArea.y * 2 / foodPerAxis) * j);
                posToSpawn.y += Random.Range(-foodRandOffset, foodRandOffset);

                Instantiate(foodPrefab, posToSpawn, Quaternion.identity, foodParent);
            }
        }
    }

    /// @brief Cria um novo inimigo na simulação.
    /// @param[in] onMousePosition Define se o inimigo deve aparecer na posição do mouse (true) ou aleatoriamente fora do campo de visão (false).
    private void SpawnEnemy(bool onMousePosition = false)
    {
        bool spawnHorizontal = Random.value > 0.5f;
        Vector3 viewportPos = Vector3.zero;

        if (spawnHorizontal)
        {
            // Fora na esquerda ou direita
            viewportPos.x = Random.value < 0.5f ? -enemySpawnPlusDist : 1 + enemySpawnPlusDist;
            viewportPos.y = Random.Range(0f, 1f);
        }
        else
        {
            // Fora em cima ou embaixo
            viewportPos.y = Random.value < 0.5f ? -enemySpawnPlusDist : 1 + enemySpawnPlusDist;
            viewportPos.x = Random.Range(0f, 1f);
        }

        // Converter de Viewport (0–1) para coordenadas do mundo
        Vector2 spawnPos = Camera.main.ViewportToWorldPoint(viewportPos);
        if (onMousePosition)
            spawnPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if ((testingEnemy || testingHunger) && !onMousePosition)
        {
            if (!testingHunger && currentTestEnemy == null)
                currentTestEnemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
        }
        else
            Instantiate(enemyPrefab, spawnPos, Quaternion.identity);

        enemySpawnTimer = timeBetweenEnemies;
    }

    /// @brief Chamado quando a IA principal morre.
    /// @details Desativa a simulação e remove inimigos do cenário.
    public void OnAIDied()
    {
        this.enabled = false;
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject e in enemies)
            e.SetActive(false);

        SimManager.instance.OnSimStepEnded();
    }

    /// @brief Desenha os limites do mapa no editor.
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(Vector3.zero, mapSize * 2);
    }
}
