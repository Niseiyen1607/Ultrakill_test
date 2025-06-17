using UnityEngine;
using System.Collections;
using SmallHedge.SoundManager; 

public class MeleeWeapon : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private Grappling grappling;
    private PlayerCam playerCam;
    public bool isAttacking { get; private set; }

    [Header("Camera Shake Setting")]
    [SerializeField] private float shakeDuration = 0.2f;
    [SerializeField] private float shakeStrength = 0.2f;
    [SerializeField] private int shakeVibrato = 10;
    [SerializeField] private float shakeRandomness = 90f;

    [Header("Melee Attack Settings")]
    [SerializeField] private Transform attackPoint; 
    [SerializeField] private float attackDamage = 20f;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackRadius = 1f;
    [SerializeField] private LayerMask enemyLayer; 
    [SerializeField] private float enemyKnockbackForce = 15f; 
    [SerializeField] private GameObject hitParticlesPrefab; 


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
        SoundManager.PlaySound(SoundType.KATANA_ATTACK);

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

                SoundManager.PlaySoundAtPosition(SoundType.ENEMIES_HIT, hitPoint);

                Rigidbody enemyRb = hitCollider.GetComponent<Rigidbody>();
                if (enemyRb != null)
                {
                    Vector3 knockbackDirection = (hitCollider.transform.position - transform.position).normalized;
                    knockbackDirection.y = 0; 
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