using UnityEngine;
using UnityEngine.UI;

public class AIStatus : MonoBehaviour
{
    // Barras de status
    public float health { get; private set; }
    public float sleep { get; private set; }
    public float hunger { get; private set; }

    public float maxStatusValue { get; private set; } = 100.0f;

    [Header("Hunger properties")]
    [SerializeField] private float hungerPerSecond;
    [SerializeField] private float starvedDamage;
    public float hungerPerFood { get; private set; } = 20.0f;

    [Header("Health properties")]
    [SerializeField] private float healthRecovery;
    [SerializeField] private float minHungerToHPRecovery;

    [Header("Sleep properties")]
    [SerializeField] private float sleepPerSecond;
    [Tooltip("While sleeping")]
    [SerializeField] private float sleepRecovery;

    [Header("Assignables")]
    [SerializeField] private AIHead ai_head;
    [SerializeField] private Slider sld_hunger;
    [SerializeField] private Slider sld_health;
    [SerializeField] private Slider sld_sleep;

    private void Awake()
    {
        // Valores iniciais
        health = maxStatusValue;
        sleep = maxStatusValue;
        hunger = maxStatusValue;

        // Setup dos sliders
        sld_hunger.maxValue = maxStatusValue;
        IncrementHunger(maxStatusValue);

        sld_health.maxValue = maxStatusValue;
        IncrementHealth(maxStatusValue);

        sld_sleep.maxValue = maxStatusValue;
        IncrementSleep(maxStatusValue);
    }

    private void Update()
    {
        if (health <= 0.0f)
            return;

        if (hunger <= 0.0f)
            DecrementHealth(starvedDamage * Time.deltaTime);
        else if (hunger >= minHungerToHPRecovery && sleep > 0.0f)
            IncrementHealth(healthRecovery * Time.deltaTime);

        DecrementHunger(hungerPerSecond * Time.deltaTime);
        DecrementSleep(sleepPerSecond * Time.deltaTime);
    }

    /// Gerenciamento dos status

    // Vida
    public void DecrementHealth(float value)
    {
        health = ClampStatusValue(health, -value);

        if (health <= 0.0)
            ai_head.OnLifeEnded();

        UpdateHealthSlider();
    }

    public void IncrementHealth(float value)
    {
        health = ClampStatusValue(health, value);

        UpdateHealthSlider();
    }

    private void UpdateHealthSlider()
    {
        sld_health.value = health;
    }

    // Sono
    public void DecrementSleep(float value)
    {
        sleep = ClampStatusValue(sleep, -value);

        UpdateSleepSlider();
    }

    public void RecoverSleep()
    {
        sleep = ClampStatusValue(sleep, sleepRecovery * Time.deltaTime * Time.timeScale);

        UpdateSleepSlider();
    }

    public void IncrementSleep(float value)
    {
        sleep = ClampStatusValue(sleep, value);

        UpdateSleepSlider();
    }

    private void UpdateSleepSlider()
    {
        sld_sleep.value = sleep;
    }

    // Fome
    public void DecrementHunger(float value)
    {
        hunger = ClampStatusValue(hunger, -value);

        UpdateHungerSlider();
    }

    public void IncrementHunger(float value)
    {
        hunger = ClampStatusValue(hunger, value);

        UpdateHungerSlider();
    }

    private void UpdateHungerSlider()
    {
        sld_hunger.value = hunger;
    }

    // General
    private float ClampStatusValue(float original, float extra)
    {
        return Mathf.Clamp(original + extra, 0.0f, maxStatusValue);
    }

    public bool IsSleepy()
    {
        return (sleep <= 0.0f);
    }

    public void SetHungerPerSecond(float value)
    {
        hungerPerSecond = value;
    }
}
