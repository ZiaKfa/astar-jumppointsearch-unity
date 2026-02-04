using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    private AudioManager audioManager;
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
    public GameObject feedbackPrefab;
    public GameObject invalidFeedbackPrefab;
    private float feedbackDuration = 0.3f;

    float fireTimer;
    bool isInvincible = false;
    
    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        audioManager = FindFirstObjectByType<AudioManager>();
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
        Feedback(tx, ty);
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
    void Feedback(int x, int y)
    {
        if (feedbackPrefab == null) return;

        // Check if the target position is an obstacle


        Vector3 pos = GridManager.Instance.GridToWorld(x, y);
        if (!GridManager.Instance.Map[x, y])
        {
        audioManager.playSfx(audioManager.invalid);
        GameObject indicator = Instantiate(
            invalidFeedbackPrefab,
            pos,
            Quaternion.Euler(0, 0, 45)
        );
        Destroy(indicator, feedbackDuration);
        } else {
        pos = pos - new Vector3(0f, 0.3f, 0f);
        audioManager.playSfx(audioManager.valid);
        GameObject indicator = Instantiate(
            feedbackPrefab,
            pos,
            Quaternion.identity
        );
        Destroy(indicator, feedbackDuration);
        }


        
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
        audioManager.playSfx(audioManager.shoot);
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
            audioManager.playSfx(audioManager.playerHit);
            Destroy(collision.gameObject);
        }

        if (collision.CompareTag("Health"))
        {
            audioManager.playSfx(audioManager.heal);
            gameController.HealPlayer(1);
            Destroy(collision.gameObject);
        }
        if (collision.CompareTag("Coin"))
        {
            audioManager.playSfx(audioManager.coin);
            gameController.AddScore(10);
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
