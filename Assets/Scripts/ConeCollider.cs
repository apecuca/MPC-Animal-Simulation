using UnityEngine;

public class ConeCollider : MonoBehaviour
{
    public Transform nearestFoodSource { get; private set; } = null;

    [Header("Assignables")]
    [SerializeField] private Transform parent;
    [SerializeField] private Transform colliderParent;

    public void UpdateRotation(Vector2 dir)
    {
        colliderParent.right = dir;
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.transform.CompareTag("Food"))
        {
            if (nearestFoodSource)
            {
                // Substituir se a fonte nova for mais perto
                float ogDist = Vector2.Distance(nearestFoodSource.position, parent.position);
                if (ogDist > Vector2.Distance(col.transform.position, parent.position))
                    nearestFoodSource = col.transform;
            }
            else
                nearestFoodSource = col.transform;
        }

    }
}
