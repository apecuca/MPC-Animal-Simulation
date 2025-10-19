using UnityEngine;

public class AICombat : MonoBehaviour
{
    [SerializeField] private float atkDamage;
    [SerializeField] private float atkCooldown;
    private float atkTimer = 0.0f;
    public float atkRange { get; private set; } = 1.0f;

    private void Update()
    {
        if (atkTimer > 0)
            atkTimer -= Time.deltaTime;
    }

    public bool IsInAtkRange(Transform parent, Transform target)
    {
        if (target == null) return false;

        return (Vector2.Distance(parent.position, target.position) <= atkRange);
    }

    public void StartAttackTimer()
    {
        atkTimer = atkCooldown;
    }

    // Retorna se o ataque teve sucesso
    public bool Attack(Transform parent, Enemy target)
    {
        if (atkTimer > 0.0f) return false;
        if (!IsInAtkRange(parent, target.transform)) return false;

        target.TakeDamage(atkDamage);
        return true;
    }
}
