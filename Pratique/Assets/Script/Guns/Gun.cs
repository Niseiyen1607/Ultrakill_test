using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    [Header("Reference")]
    public GameObject hitScanPrefab;
    public GameObject projectilePrefab;
    public Transform firePoint; // Point from where the bullet is fired
    public Animator Animator; 
    public LayerMask layerMask;

    [Header("Type")]
    public bool projectile = false; // If the fired bullet is a projectile
    public bool Automatic = false; // Hold Down the mouse button to fire continuously
    public bool destroyOnImpact = false; // If the bullet should be destroyed on impact
    public bool ExploadOnDestroy = false; // If the bullet should explode on destroy
    public bool ricochetOnHit = false; // If the bullet should ricochet on hit
    public bool canParryBullets = false; // Punch after shooting to parry your own bullets
    public bool selfDamage = false; // If the bullet should damage the player that fired it

    [Header("Properties")]
    public float damage; // Damage on Hit
    public float minDamage; // Damage Applied at max Falloff distance
    public float damageFalloff; // The time it takes to go from max damage to min damage
    public int Ammo; // bullets shot before reloading
    public float attackSpeed; // Time between shots
    public float reloadSpeed; // Time it takes to reload
    public int multishot; // Number of bullets shot at once
    public float multishotInterval; // Time between multishot bullets
    public float spread; // The amount that each bullet strays from the target
    public float knockback; // The force applied to the target from 1 bullet hit
    public float playerKnockback; // The force applied to the player from 1 bullet hit
    public int ricochetCount; // How many times the bullet can ricochet before being destroyed
    public float richochetMultiplier = 1f; // The damage multiplier for each ricochet
    public float ricochetRange = 20f; // The range at which the bullet can ricochet to another target
    public int pierceCount; // How many targets the bullet can pierce before being destroyed

    [Header("Projectile Propreties")]
    public bool inheritVelocity; // Adds the player's velocity to the projectile
    public bool projectileSticky; // Bullet get stucks to whatever they collide with
    public float projectileSpeed; // Speed of the projectile
    public float projectileGravity; // Gravity force of the projectile
    public float projectileLifeTime; // How long the projectile exists before being destroyed

    [Header("Explosion Properties")]
    public float explosionRadius; // Radius of the explosion
    public float explosionDamage; // Damage of the explosion
    public float explosionKnockback; // Knockback of the explosion

    [Header("States")]
    public bool canShoot = true; // If the gun can shoot
    public bool attackCooldown = false; // If the gun is in attack cooldown
    public bool multishotCooldown = false; // If the gun is in multishot cooldown
    public bool reloading = false; // If the gun is reloading
    public bool holdingShoot = false; // If the player is holding the shoot button
    public bool holdingAltShoot = false; // If the player is holding the alt shoot button

    [Header("info")]
    public float attackCooldownTimer = 0f; // Timer for the attack cooldown
    public float multishotCooldownTimer = 0f; // Timer for the multishot cooldown
    public float reloadTimer = 0f; // Timer for the reload
    public float currentAmmo; // Current ammo in the gun
    public float currentMultiShot;

    private float finalDamage; // Final damage after falloff


    private void Awake()
    {
        currentAmmo = Ammo;
    }

    public void TryShoot()
    {
        if (canShoot && !attackCooldown && !reloading && currentAmmo > 0)
        {
            Shoot();
        }
    }

    // ... (toutes les variables restent inchangées)

    private void Shoot()
    {
        Debug.Log("Gun Shoot");
        currentAmmo--;
        StartCoroutine(AttackCooldown());

        for (int i = 0; i < multishot; i++)
        {
            Vector3 shootDirection = ApplySpread();

            if (!projectile)
            {
                Ray ray = new Ray(Camera.main.transform.position, shootDirection);

                RaycastHit[] hits = Physics.RaycastAll(ray, 100f, layerMask);
                System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

                int pierceRemaining = pierceCount + 1;
                List<GameObject> alreadyHit = new List<GameObject>();

                foreach (RaycastHit hit in hits)
                {
                    if (pierceRemaining <= 0)
                        break;

                    GameObject hitObject = hit.collider.gameObject;
                    if (alreadyHit.Contains(hitObject))
                        continue;

                    alreadyHit.Add(hitObject);

                    Debug.Log($"Hit {hitObject.name}");
                    Debug.DrawLine(ray.origin, hit.point, Color.red, 2f);

                    if (hitScanPrefab)
                    {
                        Instantiate(hitScanPrefab, hit.point, Quaternion.LookRotation(hit.normal));
                        GameObject trail = Instantiate(hitScanPrefab);
                        TrailRenderer trailRenderer = trail.GetComponent<TrailRenderer>();
                        StartCoroutine(SpawnTrail(trailRenderer, firePoint.position, hit.point));
                    }

                    float distance = Vector3.Distance(firePoint.position, hit.point);
                    finalDamage = Mathf.Lerp(damage, minDamage, Mathf.Clamp01(distance / damageFalloff));

                    // Knockback
                    Rigidbody rb = hit.collider.GetComponent<Rigidbody>();
                    if (rb && knockback != 0)
                    {
                        Vector3 knockbackDirection = (hit.point - firePoint.position).normalized;
                        rb.AddForce(knockbackDirection * knockback, ForceMode.Impulse);
                    }

                    if (ExploadOnDestroy)
                    {
                        Explode(hit.point);
                    }

                    // Ricochet sur la première cible seulement
                    if (ricochetOnHit && ricochetCount > 0 && pierceRemaining == pierceCount + 1)
                    {
                        StartCoroutine(Ricochet(hit.point, new List<GameObject>(alreadyHit), ricochetCount));
                    }

                    pierceRemaining--;
                }

                // Si aucun hit, tirer un trail jusqu'à la fin
                if (hits.Length == 0 && hitScanPrefab)
                {
                    Vector3 endPoint = ray.origin + ray.direction * 100f;
                    GameObject trail = Instantiate(hitScanPrefab);
                    TrailRenderer trailRenderer = trail.GetComponent<TrailRenderer>();
                    StartCoroutine(SpawnTrail(trailRenderer, firePoint.position, endPoint));
                }
            }
            else
            {
                GameObject proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.LookRotation(shootDirection));
                Rigidbody rb = proj.GetComponent<Rigidbody>();
                if (rb)
                    rb.velocity = shootDirection * projectileSpeed;
            }
        }

        // Player knockback
        if (playerKnockback != 0)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player)
            {
                Rigidbody playerRb = player.GetComponent<Rigidbody>();
                if (playerRb)
                {
                    Vector3 knockbackDirection = (player.transform.position - firePoint.position).normalized;
                    Vector3 rocketJumpForce = knockbackDirection * playerKnockback + Vector3.up * playerKnockback * 0.7f;
                    playerRb.AddForce(rocketJumpForce, ForceMode.Impulse);
                }
            }
        }
    }

    public void Reload()
    {
        if (!reloading && currentAmmo < Ammo)
        {
            StartCoroutine(ReloadCoroutine());
        }
    }

    private void Explode(Vector3 position)
    {
        // Cherche tous les colliders dans le rayon d'explosion
        Collider[] colliders = Physics.OverlapSphere(position, explosionRadius, layerMask);

        foreach (Collider nearbyObject in colliders)
        {
            /* 
            Health targetHealth = nearbyObject.GetComponent<Health>();
            if (targetHealth != null)
            {
                // Calcule la distance pour diminuer les dégâts à la périphérie
                float distance = Vector3.Distance(position, nearbyObject.transform.position);
                float damageRatio = Mathf.Clamp01(1 - (distance / explosionRadius));
                float damageToApply = explosionDamage * damageRatio;

                targetHealth.TakeDamage(damageToApply);
            }
            */

            // Appliquer knockback si possible
            Rigidbody rb = nearbyObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 forceDirection = (nearbyObject.transform.position - position).normalized;
                rb.AddForce(forceDirection * explosionKnockback, ForceMode.Impulse);
            }
        }
    }

    private GameObject FindNearestEnemy(Vector3 fromPosition, List<GameObject> excludeList)
    {
        float maxDistance = ricochetRange;
        float closestDist = Mathf.Infinity;
        GameObject closest = null;

        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        foreach (GameObject enemy in enemies)
        {
            if (excludeList.Contains(enemy)) continue;

            float dist = Vector3.Distance(fromPosition, enemy.transform.position);
            if (dist < maxDistance && dist < closestDist)
            {
                closestDist = dist;
                closest = enemy;
            }
        }

        return closest;
    }

    private IEnumerator Ricochet(Vector3 fromPosition, List<GameObject> alreadyHit, int remainingRicochets)
    {
        yield return new WaitForSeconds(attackSpeed);

        GameObject nearest = FindNearestEnemy(fromPosition, alreadyHit);
        if (nearest == null)
        {
            // Aucun ennemi proche, pas de ricochet du tout
            yield break;
        }

        Vector3 dir = (nearest.transform.position - fromPosition).normalized;
        Ray ricochetRay = new Ray(fromPosition, dir);

        if (Physics.Raycast(ricochetRay, out RaycastHit hit, 100f, layerMask))
        {
            Debug.DrawLine(fromPosition, hit.point, Color.yellow, 1f);

            if (hitScanPrefab)
            {
                GameObject trail = Instantiate(hitScanPrefab);
                TrailRenderer trailRenderer = trail.GetComponent<TrailRenderer>();
                StartCoroutine(SpawnTrail(trailRenderer, fromPosition, hit.point));
                Instantiate(hitScanPrefab, hit.point, Quaternion.LookRotation(hit.normal));
            }

            Rigidbody rb = hit.collider.GetComponent<Rigidbody>();
            if (rb)
                rb.AddForce(dir * knockback, ForceMode.Impulse);

            alreadyHit.Add(hit.collider.gameObject);

            if (remainingRicochets > 1)
            {
                StartCoroutine(Ricochet(hit.point, alreadyHit, remainingRicochets - 1));
            }
        }
    }

    private IEnumerator ReloadCoroutine()
    {
        reloading = true;
        yield return new WaitForSeconds(reloadSpeed);
        currentAmmo = Ammo;
        reloading = false;
    }

    private Vector3 ApplySpread()
    {
        // Hitscan avec spread
        Vector3 shootDirection = Camera.main.transform.forward;

        // Appliquer le spread : on modifie la direction du tir
        shootDirection += new Vector3(
            Random.Range(-spread, spread),
            Random.Range(-spread, spread),
            Random.Range(-spread, spread)
        );
        shootDirection.Normalize();
        return shootDirection;
    }

    private IEnumerator AttackCooldown()
    {
        attackCooldown = true;
        yield return new WaitForSeconds(attackSpeed);
        attackCooldown = false;
    }

    private IEnumerator SpawnTrail(TrailRenderer trail, Vector3 start, Vector3 hit)
    {
        float time = 0f;

        while (time < 1)
        {
            trail.transform.position = Vector3.Lerp(start, hit, time);
            time += Time.deltaTime / trail.time;
            yield return null;
        }

        trail.transform.position = hit;
        Destroy(trail.gameObject, trail.time);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, ricochetRange);
    }
}
