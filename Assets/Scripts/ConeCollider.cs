using UnityEngine;

/// @brief Responsável por detectar objetos dentro de um cone de visão da IA.
/// @details Informa o AIHead quando detecta comida ou inimigos, e atualiza a rotação do colisor.
public class ConeCollider : MonoBehaviour
{
    [Header("Assignables")]
    [SerializeField] private Transform parent;           ///< Transform do objeto principal da IA.
    [SerializeField] private Transform colliderParent;   ///< Transform responsável pela orientação do colisor.
    [SerializeField] private AIHead ai_head;             ///< Referência ao controlador principal da IA.

    /// @brief Atualiza a rotação do colisor para alinhar com a direção atual.
    /// @param[in] dir Direção para a qual o colisor deve apontar.
    public void UpdateRotation(Vector2 dir)
    {
        colliderParent.right = dir;
    }

    /// @brief Detecta colisões com objetos dentro do cone de visão.
    /// @param[in] col Colisor detectado pela área do cone.
    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.transform.CompareTag("Food"))
            ai_head.OnFoodSourceDetected(col.transform);

        if (col.transform.CompareTag("Enemy"))
            ai_head.OnEnemySpotted(col.gameObject.GetComponent<Enemy>());
    }
}
