using UnityEngine;
using UnityEngine.UI;

/// @class AIStatus
/// @brief Gerencia os status vitais de um agente de IA, como vida, fome e sono.
/// @details Atualiza os valores de status a cada frame, aplica decaimento e recuperação,
/// e sincroniza os sliders de interface.
public class AIStatus : MonoBehaviour
{
    /// @brief Valor atual de vida do agente.
    public float health { get; private set; }

    /// @brief Valor atual de sono do agente.
    public float sleep { get; private set; }

    /// @brief Valor atual de fome do agente.
    public float hunger { get; private set; }

    /// @brief Valor máximo de qualquer status.
    public static readonly float maxStatusValue = 100.0f;

    [Header("Hunger properties")]
    /// @brief Dano por fome extrema.
    public float starvedDamage { get; private set; } = 3.2f;

    /// @brief Decaimento de fome por segundo.
    public float hungerDecay { get; private set; } = 1.25f;

    /// @brief Quantidade de fome recuperada por comida por segundo.
    public float hungerPerFood { get; private set; } = 2.0f;

    [Header("Health properties")]
    /// @brief Recuperação de vida por segundo.
    public float healthRecovery { get; private set; } = 2.4f;

    /// @brief Valor mínimo de fome para permitir recuperação de vida.
    public float minHungerToHPRecovery { get; private set; } = 20.0f;

    [Header("Sleep properties")]
    /// @brief Decaimento de sono por segundo.
    public float sleepDecay { get; private set; } = 0.75f;

    [Tooltip("While sleeping")]
    /// @brief Recuperação de sono por segundo.
    public float sleepRecovery { get; private set; } = 1.0f;

    [Header("Assignables")]
    [SerializeField] private AIHead ai_head;
    [SerializeField] private Slider sld_hunger;
    [SerializeField] private Slider sld_health;
    [SerializeField] private Slider sld_sleep;

    public static AIStatus instance { get; private set; }

    /// @brief Inicializa os valores de status e configura os sliders.
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        else
            instance = this;

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

    /// @brief Atualiza os status a cada frame, aplicando decaimento e recuperação.
    private void Update()
    {
        if (health <= 0.0f)
            return;

        if (hunger <= 0.0f)
            DecrementHealth(starvedDamage * Time.deltaTime);
        else if (hunger >= minHungerToHPRecovery && sleep > 0.0f)
            IncrementHealth(healthRecovery * Time.deltaTime);

        DecrementHunger(hungerDecay * Time.deltaTime);
        DecrementSleep(sleepDecay * Time.deltaTime);
    }

    /// Gerenciamento dos status

    // Vida
    /// @brief Reduz a vida do agente.
    /// @param[in] value Quantidade de vida a ser decrementada.
    public void DecrementHealth(float value)
    {
        health = ClampStatusValue(health, -value);

        if (health <= 0.0)
            ai_head.OnLifeEnded();

        UpdateHealthSlider();
    }

    /// @brief Aumenta a vida do agente.
    /// @param[in] value Quantidade de vida a ser incrementada.
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
    /// @brief Reduz o sono do agente.
    /// @param[in] value Quantidade de sono a ser decrementada.
    public void DecrementSleep(float value)
    {
        sleep = ClampStatusValue(sleep, -value);
        UpdateSleepSlider();
    }

    /// @brief Recupera sono durante o descanso.
    public void RecoverSleep()
    {
        // Compensar pelo decay
        sleep = ClampStatusValue(sleep, (sleepRecovery + sleepDecay) * Time.deltaTime);
        UpdateSleepSlider();
    }

    /// @brief Aumenta o sono do agente.
    /// @param[in] value Quantidade de sono a ser incrementada.
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
    /// @brief Reduz a fome do agente.
    /// @param[in] value Quantidade de fome a ser decrementada.
    public void DecrementHunger(float value)
    {
        hunger = ClampStatusValue(hunger, -value);
        UpdateHungerSlider();
    }

    /// @brief Aumenta a fome do agente.
    /// @param[in] value Quantidade de fome a ser incrementada.
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
    /// @brief Limita qualquer valor de status entre 0 e maxStatusValue.
    /// @param[in] original Valor original do status.
    /// @param[in] extra Valor adicional a ser aplicado.
    /// @return Valor do status após limite.
    public static float ClampStatusValue(float original, float extra)
    {
        return Mathf.Clamp(original + extra, 0.0f, maxStatusValue);
    }

    /// @brief Verifica se o agente está sonolento.
    /// @return Verdadeiro se o sono estiver zero.
    public bool IsSleepy()
    {
        return (sleep <= 0.0f);
    }

    /// @brief Define a taxa de fome por segundo.
    /// @param[in] value Novo valor de decaimento de fome.
    public void SetHungerPerSecond(float value)
    {
        hungerDecay = value;
    }
}
