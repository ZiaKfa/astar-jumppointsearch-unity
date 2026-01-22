using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;

    List<(int x, int y)> path;
    int pathIndex;

    int gx, gy;

    SpriteRenderer sr;
    Vector3 lastPosition;

    [Header("Auto Attack")]
    public float attackRange = 5f;
    public float fireRate = 0.5f;
    public GameObject bulletPrefab;
    public Transform firePoint;

    [Header("Invincibility")]
    public float invincibleDuration = 0.8f;

    public GameController gameController;

    float fireTimer;
    bool isInvincible = false;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();

        Vector2Int cell = GridManager.Instance.GetRandomWalkableCell();
        gx = cell.x;
        gy = cell.y;

        transform.position = GridManager.Instance.GridToWorld(gx, gy);
        lastPosition = transform.position;
    }

    void Update()
    {
        HandleInput();
        MoveAlongPath();
        HandleFlip();
        AutoShoot();
    }


    void HandleInput()
    {
        if (!Input.GetMouseButtonDown(0)) return;

        Vector3 world = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        world.z = 0;

        if (!GridManager.Instance.WorldToGrid(world, out int tx, out int ty))
            return;

        GridManager.Instance.WorldToGrid(transform.position, out gx, out gy);

        var rawPath = JumpPointSearch.FindPath(
            GridManager.Instance.Map,
            gx, gy,
            tx, ty
        );

        if (rawPath == null || rawPath.Length == 0)
            return;

        path = new List<(int, int)>(rawPath);
        pathIndex = 0;
    }

    void MoveAlongPath()
    {
        if (path == null || pathIndex >= path.Count)
            return;

        var node = path[pathIndex];
        Vector3 target = GridManager.Instance.GridToWorld(node.x, node.y);

        transform.position = Vector3.MoveTowards(
            transform.position,
            target,
            moveSpeed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, target) < 0.05f)
        {
            gx = node.x;
            gy = node.y;
            pathIndex++;
        }
    }

    void HandleFlip()
    {
        Vector3 delta = transform.position - lastPosition;

        if (Mathf.Abs(delta.x) > 0.001f)
            sr.flipX = delta.x < 0;

        lastPosition = transform.position;
    }

    void AutoShoot()
    {
        fireTimer += Time.deltaTime;
        if (fireTimer < fireRate) return;

        EnemyController target = FindNearestEnemy();
        if (target == null) return;

        Vector3 dir = (target.transform.position - firePoint.position).normalized;

        GameObject bullet = Instantiate(
            bulletPrefab,
            firePoint.position,
            Quaternion.identity
        );

        bullet.GetComponent<Bullet>().Init(dir);

        fireTimer = 0f;
    }

    EnemyController FindNearestEnemy()
    {
        EnemyController[] enemies =
            FindObjectsByType<EnemyController>(FindObjectsSortMode.None);

        float minDist = attackRange;
        EnemyController nearest = null;

        foreach (var e in enemies)
        {
            if (e == null) continue;

            float d = Vector3.Distance(transform.position, e.transform.position);
            if (d < minDist)
            {
                minDist = d;
                nearest = e;
            }
        }
        return nearest;
    }


    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            TakeDamage(1);
            Destroy(collision.gameObject);
        }

        if (collision.CompareTag("Health"))
        {
            gameController.HealPlayer(2);
            Destroy(collision.gameObject);
        }
    }

    void TakeDamage(int dmg)
    {
        if (isInvincible)
            return;

        gameController.DamagePlayer(dmg);
        StartCoroutine(InvincibleCoroutine());
    }

    IEnumerator InvincibleCoroutine()
    {
        isInvincible = true;

        float timer = 0f;
        while (timer < invincibleDuration)
        {
            sr.enabled = !sr.enabled;  
            yield return new WaitForSeconds(0.1f);
            timer += 0.1f;
        }

        sr.enabled = true;
        isInvincible = false;
    }
    public (int x, int y) GridPos => (gx, gy);
}
