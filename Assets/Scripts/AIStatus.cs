using UnityEngine;

public class AIStatus : MonoBehaviour
{
    // Barras de status
    public float life { get; private set; }
    public float sleep { get; private set; }
    public float hunger { get; private set; }

    // Valores
    public float hungerPerFood { get; private set; } = 20.0f;

    // Outras classes
    private AIHead ai_head;

    private void Awake()
    {
        ai_head = GetComponent<AIHead>();

        life = 100.0f;
        sleep = 100.0f;
        hunger = 100.0f;
    }

    /// Gerenciamento dos status

    // Vida
    public void DecrementLife(float value)
    {
        ClampStatusValue(life, value);

        if (life <= 0)
            ai_head.OnLifeEnded();
    }

    public void IncrementLife(float value)
    {
        ClampStatusValue(life, value);
    }

    // Sono
    public void DecrementSleep(float value)
    {
        ClampStatusValue(sleep, value);
    }

    public void IncreentSleep(float value)
    {
        ClampStatusValue(sleep, value);
    }

    // Fome
    public void DecrementHunger(float value)
    {
        ClampStatusValue(hunger, value);
    }

    public void IncrementHunger(float value)
    {
        ClampStatusValue(hunger, value);
    }

    // General
    private float ClampStatusValue(float original, float extra)
    {
        return Mathf.Clamp(original + extra, 0.0f, 100.0f);
    }
}
