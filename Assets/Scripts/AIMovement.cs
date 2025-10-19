using UnityEngine;

public class AIMovement : MonoBehaviour
{
    public Vector2 lastRegisteredDir { get; private set; } = Vector2.zero;

    [Header("Properties")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float sleepyMoveSpeed;

    private AIStatus ai_status;

    [Header("Assignables")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private ConeCollider coneCollider;

    private void Awake()
    {
        ai_status = GetComponent<AIStatus>();
    }

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
}
