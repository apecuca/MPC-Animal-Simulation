using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private float moveSpeed;
    [SerializeField] private float maxHealth;
    [SerializeField] private float damage;
    [SerializeField] private float atkRange;
    [SerializeField] private float atkCooldown;
    private bool attacking;
    private float atkTimer;
    private AIHead target;
    private Rigidbody2D rb;
    public float health { get; private set; }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        health = maxHealth;
        target = AIHead.instance;
    }

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

    public void TakeDamage(float vl)
    {
        health -= vl;
        if (health <= 0)
            Destroy(this.gameObject);
    }

    private void Attack()
    {
        attacking = false;

        if (!IsInAttackRange())
            return;

        target.TakeDamage(damage, this);
    }

    private bool IsInAttackRange()
    {
        return (Vector2.Distance(transform.position, target.transform.position) <= atkRange);
    }
}
