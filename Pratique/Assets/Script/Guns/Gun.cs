using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    [Header("Reference")]
    public GameObject hitScanPrefab;
    public GameObject projectilePrefab;
    public Transform firePoint; // Point from where the bullet is fired
    public LayerMask layerMask;

    [Header("Type")]
    public bool projectile = false; // If the fired bullet is a projectile
    public bool Automatic = false; // Hold Down the mouse button to fire continuously
    public bool destroyOnImpact = false; // If the bullet should be destroyed on impact
    public bool ExploadOnDestroy = false; // If the bullet should explode on destroy
    public bool ricochetOnHit = false; // If the bullet should ricochet on hit
    public bool canParryBullets = false; // Punch after shooting to parry your own bullets
    public bool igniteEnemy = false; // set enemies on fire when hit
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

    private void Shoot()
    {
        Debug.Log("Gun Shoot");
        currentAmmo--;
        StartCoroutine(AttackCooldown());

        if (!projectile)
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

            Ray ray = new Ray(Camera.main.transform.position, shootDirection);
            Vector3 hitPoint;

            if (Physics.Raycast(ray, out RaycastHit hit, 100f, layerMask))
            {
                Debug.Log($"Hit {hit.collider.name}");
                hitPoint = hit.point;

                Debug.DrawLine(ray.origin, hit.point, Color.red, 2f);

                if (hitScanPrefab)
                {
                    Instantiate(hitScanPrefab, hit.point, Quaternion.LookRotation(hit.normal));
                }

                // Calcul damage avec falloff selon distance
                float distance = Vector3.Distance(firePoint.position, hit.point);

                // Interpoler entre damage max et min selon distance / damageFalloff
                finalDamage = Mathf.Lerp(damage, minDamage, Mathf.Clamp01(distance / damageFalloff));
            }
            else
            {
                // Pas de hit, point loin devant
                hitPoint = ray.origin + ray.direction * 100f;
            }

            // Afficher le trail, même sans impact
            if (hitScanPrefab)
            {
                GameObject trail = Instantiate(hitScanPrefab);
                LineRenderer lr = trail.GetComponent<LineRenderer>();
                StartCoroutine(AnimateTrail(lr, firePoint.position, hitPoint));
            }
        }
        else
        {
            // Projectile (pas modifié ici)
            GameObject proj = Instantiate(projectilePrefab, transform.position, transform.rotation);
            Rigidbody rb = proj.GetComponent<Rigidbody>();
            if (rb)
                rb.velocity = transform.forward * projectileSpeed;
        }

        // Knockback, effets, etc.
    }
    private IEnumerator AttackCooldown()
    {
        attackCooldown = true;
        yield return new WaitForSeconds(attackSpeed);
        attackCooldown = false;
    }

    public void Reload()
    {
        if (!reloading && currentAmmo < Ammo)
        {
            StartCoroutine(ReloadCoroutine());
        }
    }

    private IEnumerator ReloadCoroutine()
    {
        reloading = true;
        yield return new WaitForSeconds(reloadSpeed);
        currentAmmo = Ammo;
        reloading = false;
    }

    private IEnumerator AnimateTrail(LineRenderer lr, Vector3 start, Vector3 end)
    {
        float duration = 0.05f; // trail animation duration
        float time = 0f;
        while (lr != null && time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            Vector3 currentPos = Vector3.Lerp(start, end, t);
            lr.SetPosition(0, start);
            lr.SetPosition(1, currentPos);
            yield return null;
        }

        if (lr != null)
        {
            lr.SetPosition(0, start);
            lr.SetPosition(1, end);
        }

        yield return new WaitForSeconds(0.1f); // wait a bit before destroying

        if (lr != null)
            Destroy(lr.gameObject);
    }

}
