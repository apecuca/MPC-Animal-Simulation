using UnityEngine;

/// @brief Controla o movimento da IA.
/// @details Gerencia a velocidade, direção e atualização do collider de detecção baseado na direção de movimento.
public class AIMovement : MonoBehaviour
{
    /// @brief Última direção de movimento registrada.
    public Vector2 lastRegisteredDir { get; private set; } = Vector2.zero;

    [Header("Properties")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float sleepyMoveSpeed;

    private AIStatus ai_status;

    [Header("Assignables")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private ConeCollider coneCollider;

    /// @brief Inicializa as referências necessárias.
    private void Awake()
    {
        ai_status = GetComponent<AIStatus>();
    }

    /// @brief Define a direção atual de movimento da IA.
    /// @param dir Direção de movimento desejada (vetor normalizado).
    public void SetMoveDir(Vector2 dir)
    {
        if (dir == Vector2.zero)
            rb.linearVelocity = Vector2.zero;
        else
        {
            float currentMvSpeed = ai_status.IsSleepy() ? sleepyMoveSpeed : moveSpeed;
            rb.linearVelocity = dir * currentMvSpeed;
            lastRegisteredDir = dir;
            coneCollider.UpdateRotation(dir);
        }
    }

    /// @brief Retorna a velocidade base de movimento.
    /// @return Valor da velocidade normal de movimento.
    public float GetMvSpeed()
    {
        return moveSpeed;
    }
}
