using UnityEngine;

/// @class AICombat
/// @brief Gerencia a lógica de combate da IA, incluindo ataque e alcance.
/// @details Lida com o cooldown de ataque, verificação de alcance e execução do ataque contra um inimigo.
public class AICombat : MonoBehaviour
{
    [SerializeField] private float atkDamage; ///< Quantidade de dano infligido por ataque.
    [SerializeField] private float atkCooldown; ///< Tempo (em segundos) entre ataques.
    private float atkTimer = 0.0f; ///< Temporizador interno para o cooldown do ataque.
    public float atkRange { get; private set; } = 1.0f; ///< O alcance máximo para a IA atacar.

    /// @brief Atualiza o temporizador de cooldown do ataque a cada frame.
    private void Update()
    {
        if (atkTimer > 0)
            atkTimer -= Time.deltaTime;
    }

    /// @brief Verifica se um alvo está dentro do alcance de ataque.
    /// @param[in] parent O Transform da IA (atacante).
    /// @param[in] target O Transform do alvo.
    /// @return Verdadeiro se o alvo estiver dentro do alcance, falso caso contrário.
    public bool IsInAtkRange(Transform parent, Transform target)
    {
        if (target == null) return false;

        return (Vector2.Distance(parent.position, target.position) <= atkRange);
    }

    /// @brief Inicia (ou reinicia) o temporizador de cooldown do ataque.
    public void StartAttackTimer()
    {
        atkTimer = atkCooldown;
    }

    /// @brief Tenta executar um ataque contra um inimigo.
    /// @details O ataque só ocorre se o cooldown tiver terminado e o alvo estiver no alcance.
    /// @param[in] parent O Transform da IA (atacante).
    /// @param[in] target O componente Enemy do alvo.
    /// @return Verdadeiro se o ataque foi bem-sucedido, falso caso contrário (ex: em cooldown).
    public bool Attack(Transform parent, Enemy target)
    {
        if (atkTimer > 0.0f) return false;
        if (!IsInAtkRange(parent, target.transform)) return false;

        target.TakeDamage(atkDamage);
        return true;
    }
}