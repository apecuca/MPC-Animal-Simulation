using UnityEngine;

/// @brief Representa uma fonte de alimento no ambiente.
/// @details Gerencia a disponibilidade do alimento, o tempo que ele dura e o intervalo de respawn após ser consumido.
public class Food : MonoBehaviour
{
    private float foodLeft;                                      ///< Quantidade de alimento restante.
    private static readonly float maxFood = 10.0f;               ///< Tempo máximo (em segundos) que o alimento dura antes de acabar.
    private static readonly float disabledDuration = 150.0f;     ///< Tempo (em segundos) para o alimento reaparecer após ser consumido.
    private float disabledTimer = 0.0f;                          ///< Contador para o tempo restante até a reativação.
    private bool disabled = false;                               ///< Indica se o alimento está temporariamente desativado.

    private BoxCollider2D col;                                   ///< Referência ao componente de colisão da comida.
    private SpriteRenderer spr;                                  ///< Referência ao renderizador do sprite da comida.

    /// @brief Inicializa o alimento e seus componentes.
    private void Awake()
    {
        col = GetComponent<BoxCollider2D>();
        spr = GetComponent<SpriteRenderer>();

        foodLeft = maxFood;
        disabledTimer = 0.0f;
        disabled = false;

        ToggleFood(true);
    }

    /// @brief Atualiza o estado do alimento a cada frame.
    /// @details Controla o temporizador de reativação quando o alimento está desativado.
    private void Update()
    {
        if (!disabled)
            return;

        // Lidar com o timer de reativação
        disabledTimer -= Time.deltaTime;
        if (disabledTimer <= 0.0f)
        {
            ToggleFood(true);
        }
    }

    /// @brief Simula o ato de comer o alimento.
    /// @return true enquanto ainda há comida disponível; false quando o alimento acabar.
    public bool Eat()
    {
        if (foodLeft <= 0)
        {
            ToggleFood(false);
            disabledTimer = disabledDuration;
            return false;
        }

        foodLeft -= Time.deltaTime;

        return true;
    }

    /// @brief Ativa ou desativa o alimento no cenário.
    /// @param[in] vl Define se o alimento deve ser ativado (true) ou desativado (false).
    private void ToggleFood(bool vl)
    {
        col.enabled = vl;
        spr.enabled = vl;
        disabled = !vl;

        if (vl)
            foodLeft = maxFood;
    }
}
