using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject[] enemyPrefab;
    public GameObject healthPrefab;
    public float enemySpawnInterval = 2f;
    public float healthSpawnInterval = 10f;

    Camera cam;
    float enemyTimer;
    float healthTimer;

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        enemyTimer += Time.deltaTime;
        if (enemyTimer >= enemySpawnInterval)
        {
            enemyTimer = 0f;
            SpawnEnemyOutsideCamera();
        }
        healthTimer += Time.deltaTime;
        if (healthTimer >= healthSpawnInterval)
        {
            healthTimer = 0f;
            SpawnHealthOutsideCamera();
        }
    }

    void SpawnEnemyOutsideCamera()
    {
        for (int i = 0; i < 100; i++) // safety loop
        {
            Vector2Int cell = GridManager.Instance.GetRandomWalkableCell();
            Vector3 world = GridManager.Instance.GridToWorld(cell.x, cell.y);

            if (IsOutsideCamera(world))
            {
                int index = Random.Range(0, enemyPrefab.Length);
                GameObject e = Instantiate(enemyPrefab[index], world, Quaternion.identity);

                Vector3 p = e.transform.position;
                p.z = -1f;
                e.transform.position = p;

                return;
            }
        }

        Debug.LogWarning("EnemySpawner: gagal menemukan posisi spawn di luar kamera.");
    }

    public void SpawnHealthOutsideCamera()
    {
        for (int i = 0; i < 100; i++) // safety loop
        {
            Vector2Int cell = GridManager.Instance.GetRandomWalkableCell();
            Vector3 world = GridManager.Instance.GridToWorld(cell.x, cell.y);

            if (IsOutsideCamera(world))
            {
                GameObject healthPack = Instantiate(healthPrefab, world, Quaternion.identity);
                Vector3 p = healthPack.transform.position;
                p.z = -1f;
                healthPack.transform.position = p;

                return;
            }
        }

        Debug.LogWarning("EnemySpawner: gagal menemukan posisi spawn HealthPack di luar kamera.");
    }
    bool IsOutsideCamera(Vector3 worldPos)
    {
        Vector3 viewport = cam.WorldToViewportPoint(worldPos);

        return viewport.x < 0f || viewport.x > 1f ||
               viewport.y < 0f || viewport.y > 1f;
    }
}
