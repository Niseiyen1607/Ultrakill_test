using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyShooter : MonoBehaviour
{
    public GameObject projectilePrefab;
    public float shootInterval = 2f;
    public float rotationSpeed = 5f;
    public float rotationOffset = 90f; // Ajoutez cette variable pour le décalage en degrés

    private Transform player;
    [SerializeField] private Transform shootPoint;

    void Start()
    {
        player = GameObject.FindWithTag("Player").transform;
        InvokeRepeating(nameof(Shoot), 1f, shootInterval);
    }

    void Update()
    {
        if (player == null)
        {
            return;
        }

        Vector3 directionToPlayerHorizontal = player.position - transform.position;
        directionToPlayerHorizontal.y = 0; // Ignorer l'axe Y pour la rotation du corps

        Quaternion targetRotation = Quaternion.LookRotation(directionToPlayerHorizontal);

        Quaternion offsetQuaternion = Quaternion.Euler(0, rotationOffset, 0);
        targetRotation *= offsetQuaternion;

        // Appliquer la rotation en douceur
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    void Shoot()
    {
        if (!player) return;

        Vector3 dirToShoot = (player.position - shootPoint.position).normalized; // Utiliser shootPoint pour le calcul initial

        GameObject proj = Instantiate(projectilePrefab, shootPoint.position, Quaternion.LookRotation(dirToShoot));

        Projectile projectile = proj.GetComponent<Projectile>();
        Rigidbody rb = proj.GetComponent<Rigidbody>();
        if (rb) rb.velocity = dirToShoot * projectile.speed;
    }
}