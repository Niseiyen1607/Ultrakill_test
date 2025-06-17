using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Projectile : MonoBehaviour
{
    public bool canBeParried = true;
    public float speed = 10f;
    public float damage = 10f;
    private Rigidbody rb;
    private bool isParried = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (!isParried)
            rb.velocity = transform.forward * speed;
    }

    public void Parry(Vector3 direction, float forceMultiplier = 1f)
    {
        if (isParried) return;

        isParried = true;
        rb.velocity = direction.normalized * speed * forceMultiplier;
        damage *= 2f; 
        gameObject.tag = "PlayerProjectile"; 
        GetComponent<Renderer>().material.color = Color.cyan;
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"Projectile collided with {collision.gameObject.name}");
        if (isParried)
        {
            IDamageable damageable = collision.collider.GetComponent<IDamageable>();
            if (damageable != null)
            {
                Vector3 hitPoint = collision.contacts[0].point;
                Vector3 hitDirection = -rb.velocity.normalized;
                damageable.TakeDamage(damage, hitPoint, hitDirection);
            }
        }
        else
        {
            IDamageable damageable = collision.collider.GetComponent<IDamageable>();
            Debug.Log($"Damageable found: {damageable != null}");
            if (damageable != null)
            {
                Vector3 hitPoint = collision.contacts[0].point;
                Vector3 hitDirection = -rb.velocity.normalized;
                damageable.TakeDamage(damage, hitPoint, hitDirection);
            }
        }

        Destroy(gameObject);
    }
}
