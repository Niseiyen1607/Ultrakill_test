using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    [Header("Reference")]
    public GameObject projectilePrefab; // Le seul préfabriqué nécessaire
    public Rigidbody playerRigidBody;
    public Transform firePoint;
    public Animator Animator;
    public GameObject shootParticule;
    public GameObject hitEffectPrefab; // Pour les particules d'impact
    public LayerMask layerMask; // Le projectile utilisera ce layer mask
    public WeaponRecoil weaponRecoil;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip shootSound;

    [Header("Weapon Type")]
    public bool Automatic = false;
    public bool destroyOnImpact = false; // Transmis au projectile
    public bool ExploadOnDestroy = false; // Transmis au projectile
    public bool ricochetOnHit = false; // Transmis au projectile
    public bool canParryBullets = false;
    public bool selfDamage = false;

    [Header("Base Properties")]
    public float damage;
    public float minDamage;
    public float damageFalloff; // Interprété comme du temps par le projectile
    public int Ammo;
    public float attackSpeed;
    public float reloadSpeed;
    public int multishot;
    public float spread;
    public float knockback;
    public float playerKnockback;
    public int ricochetCount; // Transmis au projectile
    public float richochetMultiplier = 1f; // Transmis au projectile
    public float ricochetRange = 20f; // Transmis au projectile
    public int pierceCount; // Transmis au projectile

    [Header("Projectile Properties")]
    public bool inheritVelocity;
    public bool projectileSticky;
    public float projectileSpeed;
    public float projectileGravity;
    public float projectileLifeTime;

    [Header("Explosion Properties")]
    public float explosionRadius;
    public float explosionDamage;
    public float explosionKnockback;

    [Header("States")]
    public bool canShoot = true;
    public bool attackCooldown = false;
    public bool reloading = false;
    public bool holdingShoot = false;
    public bool holdingAltShoot = false;

    [Header("Info")]
    public float currentAmmo;
    private float finalPlayerKnockback;

    private void Awake()
    {
        currentAmmo = Ammo;
        finalPlayerKnockback = playerKnockback;

        if (weaponRecoil == null)
        {
            weaponRecoil = GetComponent<WeaponRecoil>();
            if (weaponRecoil == null)
            {
                Debug.LogError("Gun script: WeaponRecoil component not found!");
            }
        }
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
        currentAmmo--;
        StartCoroutine(AttackCooldown());

        if (Animator != null) Animator.SetTrigger("Shoot");
        if (shootParticule != null) Instantiate(shootParticule, firePoint.position, firePoint.rotation);
        if (weaponRecoil != null) weaponRecoil.ApplyRecoil();
        if (audioSource != null && shootSound != null) audioSource.PlayOneShot(shootSound);

        // Applique le knockback sur le joueur
        Vector3 playerKnockbackDirection = -Camera.main.transform.forward;
        Vector3 rocketJumpForce = playerKnockbackDirection.normalized * finalPlayerKnockback + Vector3.up * finalPlayerKnockback * 0.7f;
        playerRigidBody.AddForce(rocketJumpForce, ForceMode.Impulse);

        // Tire un ou plusieurs projectiles
        for (int i = 0; i < multishot; i++)
        {
            if (projectilePrefab == null)
            {
                Debug.LogError("Projectile Prefab is not assigned in the Gun script!", this);
                continue;
            }

            // Calcule la direction de tir avec le spread
            Vector3 shootDirection = ApplySpread();

            // Instancie le projectile
            GameObject projGO = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
            Bullet projScript = projGO.GetComponent<Bullet>();

            if (projScript != null)
            {
                // Calcule la vélocité initiale (avec celle du joueur si activé)
                Vector3 initialVelocity = shootDirection * projectileSpeed;
                if (inheritVelocity)
                {
                    initialVelocity += playerRigidBody.velocity;
                }

                // Transmet toutes les stats de l'arme au projectile pour qu'il soit autonome
                projScript.Initialize(this, initialVelocity, this.transform.root.gameObject);
            }
            else
            {
                Debug.LogError("The instantiated projectile does not have a Projectile script attached!", projGO);
            }
        }
    }

    public void Reload()
    {
        if (!reloading && currentAmmo < Ammo)
        {
            if (Animator != null)
            {
                Animator.SetTrigger("Reload");
            }
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

    private Vector3 ApplySpread()
    {
        Vector3 shootDirection = Camera.main.transform.forward;
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

    // Les méthodes Explode, Ricochet, FindNearestEnemy, SpawnTrail et ApplyDamage ont été enlevées
    // car leur logique est maintenant entièrement gérée par le script Projectile.cs

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, ricochetRange);
    }
}