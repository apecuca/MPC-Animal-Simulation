using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

struct SimulationStep
{
    public float totalTime;
}

public class SimManager : MonoBehaviour
{
    public static SimManager instance { get; private set; }
    [SerializeField] private int maxSteps;
    [SerializeField] private float avTime;

    private bool watching;
    private float currentSimTime;
    List<SimulationStep> steps;

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

    private void Update()
    {
        if (watching)
            currentSimTime += Time.deltaTime;
    }

    public void StartWatching()
    {
        watching = true;
    }

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

    private float CalculateAverageTime()
    {
        float time = 0.0f;
        foreach (SimulationStep step in steps)
            time += step.totalTime;
        time /= steps.Count;

        return time;
    }
}
