using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static Vector2 mapSize { get; private set; } = new Vector2(75.0f, 75.0f);

    [Header("Food")]
    [SerializeField] private int foodPerAxis;
    [Tooltip("Min distance from wall for objects to spawn")]
    [SerializeField] private float wallDistance;
    [SerializeField] private float foodRandOffset;

    [Header("Assignables")]
    [SerializeField] private GameObject foodPrefab;
    [SerializeField] private Transform foodParent;

    private void Start()
    {
        GenerateFood();
    }

    private void GenerateFood()
    {
        Vector2 posToSpawn = Vector2.zero;
        Vector2 spawnArea = new Vector2(mapSize.x - wallDistance, mapSize.y - wallDistance);

        for (uint i = 0; i <= foodPerAxis; i++) {
            for (uint j = 0; j <= foodPerAxis; j++) {
                // Ignorar centro
                if (i == 5 && j == 5)
                    continue;

                posToSpawn.x = -spawnArea.x + ((spawnArea.x * 2 / foodPerAxis) * i);
                posToSpawn.x += Random.Range(-foodRandOffset, foodRandOffset);
                posToSpawn.y = -spawnArea.y + ((spawnArea.y * 2 / foodPerAxis) * j);
                posToSpawn.y += Random.Range(-foodRandOffset, foodRandOffset);

                Instantiate(foodPrefab, posToSpawn, Quaternion.identity, foodParent);
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(Vector3.zero, mapSize * 2);
    }
}
