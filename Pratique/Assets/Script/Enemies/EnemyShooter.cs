using SmallHedge.SoundManager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyShooter : MonoBehaviour
{
    EnemyHealth enemyHealth;

    public GameObject projectilePrefab;
    public float shootInterval = 2f;
    public float rotationSpeed = 5f;
    public float rotationOffset = 90f;
    public float attackRange = 15f;
    public bool pursuePlayer = false;
    public float moveSpeed = 3f;

    private Transform player;
    [SerializeField] private Transform shootPoint;

    private bool isInRange => Vector3.Distance(transform.position, player.position) <= attackRange;

    void Start()
    {
        enemyHealth = GetComponent<EnemyHealth>();
        player = GameObject.FindWithTag("Player").transform;
        InvokeRepeating(nameof(Shoot), 1f, shootInterval);
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (pursuePlayer && distance > attackRange)
        {
            Vector3 direction = (player.position - transform.position).normalized;
            direction.y = 0; // ne pas flotter
            transform.position += direction * moveSpeed * Time.deltaTime;
        }

        Vector3 directionToPlayerHorizontal = player.position - transform.position;
        directionToPlayerHorizontal.y = 0;

        Quaternion targetRotation = Quaternion.LookRotation(directionToPlayerHorizontal);
        Quaternion offsetQuaternion = Quaternion.Euler(0, rotationOffset, 0);
        targetRotation *= offsetQuaternion;

        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    void Shoot()
    {
        if (enemyHealth.isDead) return;

        if (!player || !isInRange) return;

        SoundManager.PlaySoundAtPosition(SoundType.ENEMY_SHOOT, transform.position);

        Vector3 dirToShoot = (player.position - shootPoint.position).normalized;
        GameObject proj = Instantiate(projectilePrefab, shootPoint.position, Quaternion.LookRotation(dirToShoot));

        Projectile projectile = proj.GetComponent<Projectile>();
        Rigidbody rb = proj.GetComponent<Rigidbody>();
        if (rb) rb.velocity = dirToShoot * projectile.speed;
    }

    private void OnDrawGizmosSelected()
    {
        if (shootPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(shootPoint.position, attackRange);
            Gizmos.DrawLine(shootPoint.position, shootPoint.position + shootPoint.forward * attackRange);
        }
        else
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
    }
}
