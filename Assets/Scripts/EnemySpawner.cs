using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject[] enemyPrefab;
    public float spawnInterval = 2f;

    Camera cam;
    float timer;

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer = 0f;
            SpawnEnemyOutsideCamera();
        }
    }

    void SpawnEnemyOutsideCamera()
    {
        for (int i = 0; i < 50; i++) // safety loop
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

    bool IsOutsideCamera(Vector3 worldPos)
    {
        Vector3 viewport = cam.WorldToViewportPoint(worldPos);

        return viewport.x < 0f || viewport.x > 1f ||
               viewport.y < 0f || viewport.y > 1f;
    }
}
