using SmallHedge.SoundManager;
using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;

    [Header("Optional Effects")]
    [SerializeField] private GameObject deathEffect;
    [SerializeField] private AudioClip damageSound;
    [SerializeField] private AudioClip deathSound;
    private PlayerCam playerCamera;

    private bool isDead = false;

    private void Start()
    {
        currentHealth = maxHealth;
        playerCamera = Camera.main.GetComponent<PlayerCam>();
    }

    public void TakeDamage(float amount, Vector3 hitPoint, Vector3 hitDirection)
    {
        if (isDead) return;

        SoundManager.PlaySound(SoundType.PLAYER_HIT);

        currentHealth -= amount;

        playerCamera.DoCameraShake(amount * 0.1f, 0.2f, 10, 20f);

        Debug.Log($"Player took {amount} damage. Remaining: {currentHealth}");

        if (damageSound)
            AudioSource.PlayClipAtPoint(damageSound, transform.position);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;

        Debug.Log("Player has died.");

        if (deathEffect)
            Instantiate(deathEffect, transform.position, Quaternion.identity);

        if (deathSound)
            AudioSource.PlayClipAtPoint(deathSound, transform.position);

        GetComponent<PlayerMovement>().enabled = false;

        LevelRestarter levelManager = FindObjectOfType<LevelRestarter>();
        levelManager.RestartLevel();
    }

    public void Heal(float amount)
    {
        if (isDead) return;

        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
    }

    public float GetHealthPercentage()
    {
        return currentHealth / maxHealth;
    }
}
