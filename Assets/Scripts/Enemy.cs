using UnityEngine;

/// @brief Representa um inimigo que persegue e ataca o agente controlado pela IA.
/// @details Controla movimentação, ataques e recebimento de dano.
public class Enemy : MonoBehaviour
{
    [SerializeField] private float moveSpeed;      ///< Velocidade de movimento do inimigo.
    [SerializeField] private float maxHealth;      ///< Quantidade máxima de vida do inimigo.
    [SerializeField] private float damage;         ///< Dano causado por ataque.
    [SerializeField] private float atkRange;       ///< Alcance de ataque do inimigo.
    [SerializeField] private float atkCooldown;    ///< Tempo de recarga entre ataques consecutivos.
    private bool attacking;                        ///< Indica se o inimigo está em estado de ataque.
    private float atkTimer;                        ///< Temporizador para o cooldown de ataque.
    private AIHead target;                         ///< Referência ao alvo principal (IA controlada).
    private Rigidbody2D rb;                        ///< Componente físico responsável pelo movimento.
    public float health { get; private set; }      ///< Vida atual do inimigo.

    /// @brief Inicializa os componentes e define o alvo principal.
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        health = maxHealth;
        target = AIHead.instance;
    }

    /// @brief Atualiza o comportamento do inimigo a cada frame.
    /// @details Controla o movimento em direção ao alvo e a execução de ataques conforme o alcance e cooldown.
    private void Update()
    {
        if (target == null)
            return;

        if (attacking)
        {
            if (atkTimer > 0.0f)
                atkTimer -= Time.deltaTime;
            else
                Attack();

            return;
        }

        // Ir até a IA
        if (IsInAttackRange())
        {
            if (!attacking)
            {
                Attack();
                attacking = true;
                atkTimer = atkCooldown;
                rb.linearVelocity = Vector2.zero;
            }
        }
        else
            rb.linearVelocity = (target.transform.position - transform.position).normalized * moveSpeed;
    }

    /// @brief Aplica dano ao inimigo.
    /// @param[in] vl Valor de dano recebido.
    public void TakeDamage(float vl)
    {
        health -= vl;
        if (health <= 0)
            Destroy(this.gameObject);
    }

    /// @brief Executa o ataque no alvo, se estiver dentro do alcance.
    private void Attack()
    {
        attacking = false;

        if (!IsInAttackRange())
            return;

        target.TakeDamage(damage, this);
    }

    /// @brief Verifica se o alvo está dentro do alcance de ataque.
    /// @return true se o alvo estiver dentro do alcance, false caso contrário.
    private bool IsInAttackRange()
    {
        return (Vector2.Distance(transform.position, target.transform.position) <= atkRange);
    }

    /// @brief Retorna o valor de dano do inimigo.
    /// @return Dano causado por ataque.
    public float GetAtkDmg()
    {
        return damage;
    }
}
