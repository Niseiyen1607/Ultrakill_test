using UnityEngine;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    private float currentHealth;

    [Header("Effects")]
    public GameObject hitEffect;
    public GameObject deathEffect;

    [Header("Ragdoll")]
    public Rigidbody[] ragdollBodies;
    public Collider[] ragdollColliders;
    public Collider mainCollider;

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
        Debug.Log($"{gameObject.name} took {amount} damage. Current health: {currentHealth}");

        // Pop-up damage number
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
        Debug.Log($"{gameObject.name} died!");

        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, transform.rotation);
        }

        SetRagdollState(true);

        foreach (var rb in ragdollBodies)
        {
            rb.AddForce(hitDirection * 30f, ForceMode.Impulse);
        }

        if (mainCollider != null)
            mainCollider.enabled = false;

        Destroy(gameObject, 10f);
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
