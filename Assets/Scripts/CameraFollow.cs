using UnityEngine;

/// @brief Controla o comportamento da câmera para seguir um alvo.
/// @details Atualiza a posição da câmera a cada frame, mantendo um deslocamento fixo em relação ao alvo.
public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform followTarget;
    [SerializeField] private Vector3 offset;

    /// @brief Atualiza a posição da câmera conforme o alvo definido.
    private void Update()
    {
        if (followTarget == null)
            return;

        transform.position = followTarget.position + offset;
    }
}
