using SmallHedge.SoundManager;
using UnityEngine;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;

    [Header("Effects")]
    [SerializeField] private GameObject hitEffect;
    [SerializeField] private GameObject deathEffect;

    [Header("Ragdoll")]
    [SerializeField] private float pushForce = 1f;
    [SerializeField] private float upwardForceAmount = 1f;
    [SerializeField] private Rigidbody[] ragdollBodies;
    [SerializeField] private Collider[] ragdollColliders;
    [SerializeField] private Collider mainCollider;

    [SerializeField] private RobotWalker LeftLeg;
    [SerializeField] private RobotWalker RightLeg;
    [SerializeField] private IKArm LeftArm;
    [SerializeField] private IKArm RightArm;

    public bool isDead = false;

    private void Awake()
    {
        ragdollBodies = GetComponentsInChildren<Rigidbody>();
        var allColliders = GetComponentsInChildren<Collider>();
        ragdollColliders = System.Array.FindAll(allColliders, col => col != mainCollider);

        currentHealth = maxHealth;
        SetRagdollState(false);
    }

    public void TakeDamage(float amount, Vector3 hitPoint, Vector3 hitDirection)
    {
        currentHealth -= amount;

        if (DamagePopUpGenerator.Instance != null)
        {
            DamagePopUpGenerator.Instance.CreatePopUp(hitPoint, Mathf.RoundToInt(amount).ToString(), Color.red);
        }

        if (hitEffect != null)
        {
            Instantiate(hitEffect, hitPoint, Quaternion.LookRotation(hitDirection));
        }

        if (currentHealth <= 0f)
        {
            Die(hitPoint, hitDirection);
        }
    }

    private void Die(Vector3 hitPoint, Vector3 hitDirection)
    {
        if (isDead) return; 
        isDead = true;

        Debug.Log($"{gameObject.name} died!");

        if (LeftLeg != null) LeftLeg.enabled = false;
        if (RightLeg != null) RightLeg.enabled = false;
        if (LeftArm != null) LeftArm.enabled = false;
        if (RightArm != null) RightArm.enabled = false;

        SoundManager.PlaySoundAtPosition(SoundType.ENEMY_DEATH, transform.position);

        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, transform.rotation);
        }

        SetRagdollState(true);

        Vector3 upwardForce = Vector3.up * upwardForceAmount;
        Vector3 finalForce = (hitDirection.normalized + upwardForce).normalized * pushForce;

        foreach (var rb in ragdollBodies)
        {
            rb.AddForce(finalForce, ForceMode.Impulse);
        }


        if (mainCollider != null)
            mainCollider.enabled = false;

        Destroy(gameObject, 3f);
    }

    private void SetRagdollState(bool isRagdoll)
    {
        foreach (var rb in ragdollBodies)
        {
            if (mainCollider != null && rb.gameObject == mainCollider.gameObject)
                continue; 

            rb.isKinematic = !isRagdoll;
        }

        foreach (var col in ragdollColliders)
        {
            col.enabled = isRagdoll;
        }
    }

}
