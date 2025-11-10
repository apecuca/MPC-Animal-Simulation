using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System;
using Unity.VisualScripting;
using System.Linq;

[System.Serializable]
struct SimData
{
    public int totalSimulations;
    public float averageTime;
    public float averageHealth;
    public float averageHunger;
    public float averageSleep;
    public float shortestTime;
    public int shortestID;
    public float longestTime;
    public int longestID;
    public float averageActions;
}

[System.Serializable]
public enum SimScenario
{
    DEFAULT = 0, // Todos os status cheios
    HALFSTATUS = 1, // Todos os status pela metade
    ZEROSUPPORT = 2, // Fome e sono zerados
}

/// @brief Representa uma etapa individual da simulação.
/// @details Armazena o tempo total que a IA sobreviveu em um ciclo da simulação.
struct SimulationStep
{
    /// @brief Tempo total de duração da simulação atual.
    public float totalTime;

    public int recordCount;
    public float totalHealth;
    public float totalHunger;
    public float totalSleep;

    public int actionChanges;
}

/// @brief Controla o ciclo geral da simulação.
/// @details Responsável por monitorar o tempo de execução de cada etapa, calcular médias e reiniciar a cena entre execuções.
public class SimManager : MonoBehaviour
{
    /// @brief Instância global única do SimManager.
    public static SimManager instance { get; private set; }

    [Header("Control")]
    [SerializeField] private int maxSteps;

    [Header("Simulation values")]
    [SerializeField] private bool runAllScenarios;
    [SerializeField] private SimScenario currentScenario;
    [SerializeField] private SimData currentSimData;

    private AIStatus ai_status;

    private bool watching;
    private float recordTimer;
    private float recordOffset = 0.5f; // Em segundos. Quanto menor o valor, maior a precisão da média
    private List<SimulationStep> steps;
    private SimulationStep currentStep;

    private string exportPath => Application.persistentDataPath;

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
        ResetSimData();
    }

    /// @brief Atualiza o temporizador da simulação e trata reinicializações manuais.
    private void Update()
    {
        if (watching)
        {
            currentStep.totalTime += Time.deltaTime;
            recordTimer -= Time.deltaTime;
            if (recordTimer <= 0.0f)
            {
                currentStep.totalHealth += ai_status.health;
                currentStep.totalHunger += ai_status.hunger;
                currentStep.totalSleep += ai_status.sleep;
                currentStep.recordCount++;

                recordTimer = recordOffset;
            }
        }

        // Reiniciar cena manualmente
        if (Input.GetKeyDown(KeyCode.R))
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// @brief Inicia a contagem de tempo da simulação atual.
    public void StartWatching()
    {
        ai_status = AIStatus.instance;
        switch(currentScenario)
        {
            case SimScenario.DEFAULT: break;

            case SimScenario.HALFSTATUS:
                ai_status.DecrementHunger(AIStatus.maxStatusValue / 2);
                ai_status.DecrementSleep(AIStatus.maxStatusValue / 2);
                break;

            case SimScenario.ZEROSUPPORT:
                ai_status.DecrementHunger(AIStatus.maxStatusValue);
                ai_status.DecrementSleep(AIStatus.maxStatusValue);
                break;

            default: break;
        }

        InitializeNewStep();
        recordTimer = 0.0f;
        watching = true;
    }

    /// @brief Finaliza a simulação atual e armazena o resultado.
    /// @details Calcula o tempo total, atualiza a média e reinicia a cena até atingir o número máximo de execuções.
    public void OnSimStepEnded()
    {
        watching = false;

        currentSimData.totalSimulations++;

        steps.Add(currentStep);

        // Calcular valores
        CalculateAverage();
        if (currentStep.totalTime > currentSimData.longestTime)
        {
            currentSimData.longestTime = currentStep.totalTime;
            currentSimData.longestID = steps.Count - 1;
        }
        
        if (currentStep.totalTime < currentSimData.shortestTime)
        {
            currentSimData.shortestTime = currentStep.totalTime;
            currentSimData.shortestID = steps.Count - 1;
        }

        // Cleanup
        InitializeNewStep();

        if (steps.Count < maxSteps)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            return; // Ir para próximo cenário
        }

        // Ao fim do cenário, exportar
        ExportCurrentData();

        // Ainda não está no último cenário, continuar
        if (currentScenario != Enum.GetValues(typeof(SimScenario)).Cast<SimScenario>().Max() &&
            runAllScenarios) 
        {
            ResetSimData();
            currentScenario++;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            return;
        }

        // Acabaram todos os cenários, finalizar
        EndSimulation(); 
    }

    public void OnActionChange()
    {
        if (watching)
            currentStep.actionChanges++;
    }

    private void CalculateAverage()
    {
        currentSimData.averageTime = 0.0f;
        currentSimData.averageHealth = 0.0f;
        currentSimData.averageHunger = 0.0f;
        currentSimData.averageSleep = 0.0f;

        foreach(SimulationStep step in steps)
        {
            currentSimData.averageTime += step.totalTime;
            currentSimData.averageHealth += step.totalHealth / step.recordCount;
            currentSimData.averageHunger += step.totalHunger / step.recordCount;
            currentSimData.averageSleep += step.totalSleep / step.recordCount;
            currentSimData.averageActions += step.actionChanges;
        }

        // Média geral
        currentSimData.averageTime /= steps.Count;
        currentSimData.averageHealth /= steps.Count;
        currentSimData.averageHunger /= steps.Count;
        currentSimData.averageSleep /= steps.Count;
        currentSimData.averageActions /= steps.Count;
    }

    private void InitializeNewStep()
    {
        currentStep = new SimulationStep();

        currentStep.totalTime = 0.0f;

        currentStep.recordCount = 0;
        currentStep.totalHealth = 0.0f;
        currentStep.totalHunger = 0.0f;
        currentStep.totalSleep = 0.0f;

        currentStep.actionChanges = 0;
    }

    private void ExportCurrentData()
    {
        // Exportar resultados
        string jsonStr = JsonUtility.ToJson(currentSimData, true);
        string fileName = SceneManager.GetActiveScene().name + "_" + currentScenario.ToString() + "_data.json";
        string pathToExport = Path.Combine(exportPath, fileName);
        File.WriteAllText(pathToExport, jsonStr);
    }

    private void EndSimulation()
    {
        // Resetar dados da sim antiga
        ResetSimData();
    }

    private void ResetSimData()
    {
        currentSimData = new SimData();
        steps = new List<SimulationStep>();
        currentStep = new SimulationStep();

        currentSimData.averageTime = 0.0f;
        currentSimData.averageHealth = 0.0f;
        currentSimData.averageHunger = 0.0f;
        currentSimData.averageSleep = 0.0f;
        currentSimData.shortestTime = float.MaxValue;
        currentSimData.shortestID = -1;
        currentSimData.longestTime = -float.MaxValue;
        currentSimData.longestID = -1;
        currentSimData.averageActions = 0.0f;
    }
}
