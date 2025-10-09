using UnityEngine;

public class AIMovement : MonoBehaviour
{
    public Vector2 lastRegisteredDir { get; private set; } = Vector2.zero;

    [Header("Properties")]
    [SerializeField] private float moveSpeed;

    [Header("Assignables")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private ConeCollider coneCollider;

    public void SetMoveDir(Vector2 dir)
    {
        if (dir == Vector2.zero)
            rb.linearVelocity = Vector2.zero;
        else
        {
            rb.linearVelocity = dir * moveSpeed;
            lastRegisteredDir = dir;
            coneCollider.UpdateRotation(dir);
        }
    }
}
