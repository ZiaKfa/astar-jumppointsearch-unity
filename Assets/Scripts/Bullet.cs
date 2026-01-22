using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 10f;
    public float lifeTime = 2f;
    private GameController gameController;
    Vector3 dir;

    // Prefab default menghadap kanan-atas (45 derajat)
    const float SPRITE_ANGLE_OFFSET = -45f;
    void Start()
    {
        gameController = FindFirstObjectByType<GameController>();
    }
    public void Init(Vector3 direction)
    {
        dir = direction.normalized;

        // Rotasi agar panah menghadap arah tembakan
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle + SPRITE_ANGLE_OFFSET);

        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        transform.position += dir * speed * Time.deltaTime;
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Enemy"))
        {
            gameController.AddScore(10);
            Destroy(col.gameObject);
            Destroy(gameObject);
        }
        else if (col.CompareTag("Obstacle"))
        {
            Destroy(gameObject);
        }
    }
}
