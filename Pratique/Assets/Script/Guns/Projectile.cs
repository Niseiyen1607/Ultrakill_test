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

        // Donne une vitesse initiale vers l'avant
        if (!isParried)
            rb.velocity = transform.forward * speed;
    }

    public void Parry(Vector3 direction, float forceMultiplier = 1f)
    {
        if (isParried) return;

        isParried = true;
        rb.velocity = direction.normalized * speed * forceMultiplier;
        damage *= 2f; // double les dégâts après parry
        gameObject.tag = "PlayerProjectile"; // change la cible
        GetComponent<Renderer>().material.color = Color.cyan; // change couleur pour voir que c'est parried
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Optionnel : détecte si ça touche un ennemi ou autre ici

        Destroy(gameObject);
    }
}
