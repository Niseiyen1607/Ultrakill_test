using DG.Tweening;
using SmallHedge.SoundManager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class Bullet : MonoBehaviour 
{
    private Rigidbody rb;
    private GameObject owner;
    private LayerMask layerMask;
    [SerializeField] private LayerMask predefinedRicochetLayer;
    [SerializeField] private GameObject ricochetEffectPrefab;

    private float damage;
    private float minDamage;
    private float damageFalloffTime;
    private float knockback;
    private bool isSticky;

    // Pierce
    private int pierceCount;

    // Explosion
    private bool explodeOnDestroy;
    private float explosionRadius;
    private float explosionDamage;
    private float explosionKnockback;

    // Ricochet
    private bool ricochetOnHit;
    private int ricochetCount;
    private float ricochetRange;
    private float richochetMultiplier;
    private GameObject hitEffectPrefab;

    private float timeAlive = 0f;
    private List<GameObject> alreadyHit = new List<GameObject>();

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Initialize(Gun gunStats, Vector3 initialVelocity, GameObject owner)
    {
        // Stats générales
        this.damage = gunStats.damage;
        this.minDamage = gunStats.minDamage;
        this.damageFalloffTime = gunStats.damageFalloff;
        this.knockback = gunStats.knockback;
        this.isSticky = gunStats.projectileSticky;
        this.layerMask = gunStats.layerMask;
        this.owner = owner;

        // Pierce
        this.pierceCount = gunStats.pierceCount;

        // Explosion
        this.explodeOnDestroy = gunStats.ExploadOnDestroy;
        this.explosionRadius = gunStats.explosionRadius;
        this.explosionDamage = gunStats.explosionDamage;
        this.explosionKnockback = gunStats.explosionKnockback;

        // Ricochet
        this.ricochetOnHit = gunStats.ricochetOnHit;
        this.ricochetCount = gunStats.ricochetCount;
        this.ricochetRange = gunStats.ricochetRange;
        this.richochetMultiplier = gunStats.richochetMultiplier;
        this.hitEffectPrefab = gunStats.hitEffectPrefab; 

        // Physique
        rb.velocity = initialVelocity;
        StartCoroutine(ApplyGravity(gunStats.projectileGravity));
        Destroy(gameObject, gunStats.projectileLifeTime);
    }

    void Update()
    {
        timeAlive += Time.deltaTime;
    }

    private void OnCollisionEnter(Collision collision)
    {
        int hitLayer = collision.gameObject.layer;

        if (((1 << hitLayer) & predefinedRicochetLayer) != 0)
        {
            RicochetPoint rp = collision.gameObject.GetComponent<RicochetPoint>();
            if (rp != null)
            {
                Debug.Log("Hit predefined ricochet point");
                StartCoroutine(FollowRicochetPath(rp));
                return;
            }
        }
        else if (((1 << hitLayer) & layerMask) != 0)
        {
            HandleHit(collision);
        }
        else if (!isSticky)
        {
            if (explodeOnDestroy) Explode(collision.contacts[0].point);
            Destroy(gameObject);
        }
    }

    private void HandleHit(Collision collision)
    {
        Debug.Log($"Bullet hit: {collision.gameObject.name} at {collision.contacts[0].point}");

        // Instancie les particules d'impact
        if (hitEffectPrefab)
        {
            Instantiate(hitEffectPrefab, collision.contacts[0].point, Quaternion.LookRotation(collision.contacts[0].normal));
        }

        SoundManager.PlaySoundAtPosition(SoundType.BULLET_HIT, collision.contacts[0].point, 0.3f);

        // --- Dégâts et Knockback ---
        float currentDamage = Mathf.Lerp(damage, minDamage, Mathf.Clamp01(timeAlive / damageFalloffTime));
        ApplyDamage(collision.collider, currentDamage, collision.contacts[0].point, rb.velocity.normalized);
        if (collision.rigidbody)
        {
            collision.rigidbody.AddForce(rb.velocity.normalized * knockback, ForceMode.Impulse);
        }

        alreadyHit.Add(collision.gameObject);

        // 1. Tenter le Ricochet 
        bool isFirstHit = alreadyHit.Count == 1;
        if (isFirstHit && ricochetOnHit && ricochetCount > 0)
        {
            if (TryRicochet(collision.transform.position))
            {
                this.damage *= richochetMultiplier;
                this.minDamage *= richochetMultiplier;
                ricochetCount--;
                return;
            }
        }

        pierceCount--;
        if (pierceCount < 0)
        {
            if (isSticky)
            {
                rb.isKinematic = true;
                transform.SetParent(collision.transform);
            }
            else
            {
                if (explodeOnDestroy) Explode(collision.contacts[0].point);
                Destroy(gameObject);
            }
        }
    }

    private bool TryRicochet(Vector3 fromPosition)
    {
        GameObject nearest = FindNearestEnemy(fromPosition);
        if (nearest != null)
        {
            Vector3 directionToTarget = (nearest.transform.position - transform.position).normalized;
            rb.velocity = directionToTarget * rb.velocity.magnitude; 
            return true;
        }
        return false;
    }

    private GameObject FindNearestEnemy(Vector3 fromPosition)
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject closest = null;
        float minDistance = ricochetRange;

        foreach (var enemy in enemies)
        {
            if (alreadyHit.Contains(enemy)) continue;

            float distance = Vector3.Distance(fromPosition, enemy.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closest = enemy;
            }
        }
        return closest;
    }

    private IEnumerator FollowRicochetPath(RicochetPoint startPoint)
    {
        RicochetPoint current = startPoint;

        while (current != null && current.nextTarget != null)
        {
            Vector3 direction = (current.nextTarget.position - transform.position).normalized;

            if (ricochetEffectPrefab != null)
            {
                Instantiate(ricochetEffectPrefab, transform.position, Quaternion.LookRotation(direction));
            }

            rb.velocity = direction * rb.velocity.magnitude;

            SoundManager.PlaySoundAtPosition(SoundType.RICOCHET, transform.position);

            yield return transform.DOMove(current.nextTarget.position, 0.1f)
                                          .SetEase(Ease.Linear)
                                          .WaitForCompletion();

            Collider[] hits = Physics.OverlapSphere(transform.position, 0.1f);
            foreach (var col in hits)
            {
                ObjectTriggerOnBullet trigger = col.GetComponent<ObjectTriggerOnBullet>();
                if (trigger != null)
                {
                    Debug.Log("Cristal touché via ricochet !");
                    trigger.Activate();
                    break;
                }
            }

            current = current.nextTarget.GetComponent<RicochetPoint>();
        }

        if (explodeOnDestroy)
            Explode(transform.position);
        Destroy(gameObject);
    }


    private void OnDestroy()
    {
        if (explodeOnDestroy && pierceCount >= 0)
        {
            Explode(transform.position);
        }
    }

    private void Explode(Vector3 position)
    {
        Collider[] colliders = Physics.OverlapSphere(position, explosionRadius, layerMask);
        foreach (Collider nearbyObject in colliders)
        {
            if (nearbyObject.gameObject == owner) continue;
            float distance = Vector3.Distance(position, nearbyObject.transform.position);
            float damageRatio = Mathf.Clamp01(1 - (distance / explosionRadius));
            float damageToApply = explosionDamage * damageRatio;

            ApplyDamage(nearbyObject, damageToApply, nearbyObject.transform.position,
                        (nearbyObject.transform.position - position).normalized);

            if (nearbyObject.attachedRigidbody != null)
            {
                nearbyObject.attachedRigidbody.AddForce((nearbyObject.transform.position - position).normalized * explosionKnockback, ForceMode.Impulse);
            }
        }
    }

    private IEnumerator ApplyGravity(float gravityScale)
    {
        while (true)
        {
            if (!rb.isKinematic)
            {
                rb.AddForce(Vector3.down * gravityScale, ForceMode.Acceleration);
            }
            yield return new WaitForFixedUpdate();
        }
    }

    private void ApplyDamage(Collider col, float amount, Vector3 hitPoint, Vector3 hitDir)
    {
        IDamageable dmg = col.GetComponent<IDamageable>() ?? col.GetComponentInParent<IDamageable>();
        if (dmg != null)
            dmg.TakeDamage(amount, hitPoint, hitDir);
    }
}