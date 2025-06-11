using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class WeaponSway : MonoBehaviour
{
    [Header("Sway Settings")]
    [Tooltip("Intensité du balancement horizontal (souris X)")]
    public float swayAmountX = 0.5f;
    [Tooltip("Intensité du balancement vertical (souris Y)")]
    public float swayAmountY = 0.5f;
    [Tooltip("Vitesse à laquelle l'arme revient à sa position neutre")]
    public float swaySmoothness = 5f;

    // Référence à la rotation d'origine de l'arme
    private Quaternion initialRotation;

    void Start()
    {
        // Enregistre la rotation locale initiale de l'arme
        initialRotation = transform.localRotation;
    }

    void Update()
    {
        // Récupère les entrées de la souris
        float mouseX = Input.GetAxis("Mouse X") * swayAmountX;
        float mouseY = Input.GetAxis("Mouse Y") * swayAmountY;

        // Calcul de la rotation cible pour le sway
        Quaternion targetSwayRotation = Quaternion.Euler(
            -Mathf.Clamp(mouseY, -swayAmountY, swayAmountY), // Pour le Y, inverse la direction pour un effet réaliste
            Mathf.Clamp(mouseX, -swayAmountX, swayAmountX),
            0f
        );

        // Applique le balancement en douceur
        transform.localRotation = Quaternion.Slerp(transform.localRotation, initialRotation * targetSwayRotation, Time.deltaTime * swaySmoothness);
    }
}
