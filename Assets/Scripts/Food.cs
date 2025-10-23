using UnityEngine;

public class Food : MonoBehaviour
{
    private float foodLeft;
    private static readonly float maxFood = 10.0f; // Quantos segundos ela vai durar
    private static readonly float disabledDuration = 90.0f; // Em segundos
    private float disabledTimer = 0.0f;
    private bool disabled = false;

    private BoxCollider2D col;
    private SpriteRenderer spr;

    private void Awake()
    {
        col = GetComponent<BoxCollider2D>();
        spr = GetComponent<SpriteRenderer>();

        foodLeft = maxFood;
        disabledTimer = 0.0f;
        disabled = false;

        ToggleFood(true);
    }

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

    private void ToggleFood(bool vl)
    {
        col.enabled = vl;
        spr.enabled = vl;
        disabled = !vl;
    }
}
