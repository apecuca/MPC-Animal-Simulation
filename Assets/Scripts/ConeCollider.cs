using UnityEngine;

public class ConeCollider : MonoBehaviour
{
    [Header("Assignables")]
    [SerializeField] private Transform parent;
    [SerializeField] private Transform colliderParent;
    [SerializeField] private AIHead ai_head;

    public void UpdateRotation(Vector2 dir)
    {
        colliderParent.right = dir;
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.transform.CompareTag("Food"))
            ai_head.OnFoodSourceDetected(col.transform);

    }
}
