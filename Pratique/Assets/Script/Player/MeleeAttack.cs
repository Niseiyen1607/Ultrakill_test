using UnityEngine;
using System.Collections; // N'oubliez pas d'inclure System.Collections pour les Coroutines

public class MeleeWeapon : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private Grappling grappling;
    private PlayerCam playerCam;

    private bool isAttacking = false;

    [Header("Camera Shake Setting")]
    [SerializeField] private float shakeDuration = 0.2f;
    [SerializeField] private float shakeStrength = 0.2f;
    [SerializeField] private int shakeVibrato = 10;
    [SerializeField] private float shakeRandomness = 90f;

    [Header("Melee Attack Settings")]
    [SerializeField] private Transform attackPoint; // The point where the attack is performed, usually at the front of the player
    [SerializeField] private float attackDamage = 20f;
    [SerializeField] private float attackRange = 2f; // How far the melee attack reaches
    [SerializeField] private float attackRadius = 1f; // Radius of the melee attack sphere
    [SerializeField] private LayerMask enemyLayer; // Layer for your enemies
    [SerializeField] private float enemyKnockbackForce = 15f; // How much to push enemies back
    [SerializeField] private GameObject hitParticlesPrefab; // Assign your particle system prefab here

    private void Awake()
    {
        playerCam = FindObjectOfType<PlayerCam>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !isAttacking)
        {
            PlayRandomAttackAnimation();
            PerformMeleeAttack();
        }
    }

    public void PlayRandomAttackAnimation()
    {
        isAttacking = true;

        int randomAttack = Random.Range(1, 4);

        string triggerName = "Attack" + randomAttack;
        animator.SetTrigger(triggerName);

        playerCam.DoCameraShake(shakeDuration, shakeStrength, shakeVibrato, shakeRandomness);
    }

    public void GrappingAnimation()
    {
        isAttacking = true;

        animator.SetTrigger("Attack1");

        playerCam.DoCameraShake(shakeDuration, shakeStrength, shakeVibrato, shakeRandomness);
    }

    public void PerformMeleeAttack()
    {
        Vector3 sphereOrigin = attackPoint.position + attackPoint.forward * attackRange;

        Collider[] hitColliders = Physics.OverlapSphere(sphereOrigin, attackRadius, enemyLayer);

        Debug.Log($"Melee attack hit {hitColliders.Length} enemies.");

        foreach (var hitCollider in hitColliders)
        {
            IDamageable damageable = hitCollider.GetComponent<IDamageable>();
            if (damageable != null)
            {
                Vector3 hitPoint = hitCollider.ClosestPoint(transform.position);
                Vector3 hitDirection = (hitCollider.transform.position - transform.position).normalized;

                damageable.TakeDamage(attackDamage, hitPoint, hitDirection);

                Rigidbody enemyRb = hitCollider.GetComponent<Rigidbody>();
                if (enemyRb != null)
                {
                    Vector3 knockbackDirection = (hitCollider.transform.position - transform.position).normalized;
                    knockbackDirection.y = 0; // Keep knockback horizontal for consistency, adjust if vertical knockback is desired
                    enemyRb.AddForce(knockbackDirection * enemyKnockbackForce, ForceMode.Impulse);
                }

                if (hitParticlesPrefab != null)
                {
                    Instantiate(hitParticlesPrefab, hitPoint, Quaternion.LookRotation(hitDirection));
                }
            }
        }
    }

    public void EndAttack()
    {
        isAttacking = false;
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint != null) 
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position + attackPoint.forward * attackRange, attackRadius);

            Gizmos.color = Color.green;
            Gizmos.DrawSphere(attackPoint.position, 0.1f);
        }
        else
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position + transform.forward * attackRange, attackRadius);
        }
    }
}