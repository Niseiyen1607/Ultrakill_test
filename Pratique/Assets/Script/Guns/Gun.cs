using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    [Header("Reference")]
    public GameObject projectilePrefab;
    public Rigidbody playerRigidBody;
    public Transform firePoint;
    public Animator Animator;
    public GameObject shootParticule;
    public GameObject hitEffectPrefab;
    public LayerMask layerMask; 
    public WeaponRecoil weaponRecoil;
    public PlayerCam playerCam;
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

    [Header("Camera Shake Setting")]
    [SerializeField] private float shakeDuration = 0.2f;
    [SerializeField] private float shakeStrength = 0.2f;
    [SerializeField] private int shakeVibrato = 10;
    [SerializeField] private float shakeRandomness = 90f;

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

        playerCam = FindObjectOfType<PlayerCam>();
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
        playerCam.DoCameraShake(shakeDuration, shakeStrength, shakeVibrato, shakeRandomness);

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

            Vector3 shootDirection = GetShootDirectionFromCenter();
            GameObject projGO = Instantiate(projectilePrefab, firePoint.position, Quaternion.LookRotation(shootDirection));

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

    private Vector3 GetShootDirectionFromCenter()
    {
        Vector3 direction = Camera.main.transform.forward;

        Ray ray = new Ray(Camera.main.transform.position, direction);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 1000f, layerMask))
        {
            direction = (hit.point - firePoint.position).normalized;
        }
        else
        {
            direction = (Camera.main.transform.position + Camera.main.transform.forward * 1000f - firePoint.position).normalized;
        }

        direction += new Vector3(
            Random.Range(-spread, spread),
            Random.Range(-spread, spread),
            Random.Range(-spread, spread)
        );

        return direction.normalized;
    }

    private IEnumerator AttackCooldown()
    {
        attackCooldown = true;
        yield return new WaitForSeconds(attackSpeed);
        attackCooldown = false;
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, ricochetRange);
    }
}