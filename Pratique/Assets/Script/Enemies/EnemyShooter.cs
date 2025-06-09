using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyShooter : MonoBehaviour
{
    public GameObject projectilePrefab;
    public float shootInterval = 2f;

    private Transform player;
    [SerializeField] private Transform shootPoint;

    void Start()
    {
        player = GameObject.FindWithTag("Player").transform;
        InvokeRepeating(nameof(Shoot), 1f, shootInterval);
    }

    void Shoot()
    {
        if (!player) return;

        Vector3 dir = (player.position - transform.position).normalized;
        GameObject proj = Instantiate(projectilePrefab, shootPoint.position, Quaternion.LookRotation(dir));

        Projectile projectile = proj.GetComponent<Projectile>();
        Rigidbody rb = proj.GetComponent<Rigidbody>();
        if (rb) rb.velocity = dir * projectile.speed;
    }
}
