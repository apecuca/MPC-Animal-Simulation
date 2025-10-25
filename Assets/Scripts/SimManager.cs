using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// @brief Representa uma etapa individual da simulação.
/// @details Armazena o tempo total que a IA sobreviveu em um ciclo da simulação.
struct SimulationStep
{
    /// @brief Tempo total de duração da simulação atual.
    public float totalTime;
}

/// @brief Controla o ciclo geral da simulação.
/// @details Responsável por monitorar o tempo de execução de cada etapa, calcular médias e reiniciar a cena entre execuções.
public class SimManager : MonoBehaviour
{
    /// @brief Instância global única do SimManager.
    public static SimManager instance { get; private set; }

    [SerializeField] private int maxSteps;
    [SerializeField] private float avTime;

    private bool watching;
    private float currentSimTime;
    private List<SimulationStep> steps;

    /// @brief Inicializa o SimManager e configura os valores iniciais.
    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this.gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(this.gameObject);

        // Valores iniciais
        steps = new List<SimulationStep>();
        currentSimTime = 0.0f;
        watching = false;
    }

    /// @brief Atualiza o temporizador da simulação e trata reinicializações manuais.
    private void Update()
    {
        if (watching)
            currentSimTime += Time.deltaTime;

        // Reiniciar cena manualmente
        if (Input.GetKeyDown(KeyCode.R))
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// @brief Inicia a contagem de tempo da simulação atual.
    public void StartWatching()
    {
        watching = true;
    }

    /// @brief Finaliza a simulação atual e armazena o resultado.
    /// @details Calcula o tempo total, atualiza a média e reinicia a cena até atingir o número máximo de execuções.
    public void OnSimStepEnded()
    {
        watching = false;

        SimulationStep newStep;
        newStep.totalTime = currentSimTime;
        steps.Add(newStep);

        avTime = CalculateAverageTime();

        // Cleanup
        currentSimTime = 0.0f;

        if (steps.Count < maxSteps)
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// @brief Calcula o tempo médio de sobrevivência entre as etapas concluídas.
    /// @return Média de tempo (em segundos) entre todas as simulações já executadas.
    private float CalculateAverageTime()
    {
        float time = 0.0f;
        foreach (SimulationStep step in steps)
            time += step.totalTime;
        time /= steps.Count;

        return time;
    }
}
