using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateItem : MonoBehaviour
{
    [SerializeField] private GameObject itemToActivate;
    [SerializeField] bool weaponToActivate = false;

    private GunManager gunManager;

    // Paramètres de l’oscillation
    [Header("Oscillation Settings")]
    public float floatAmplitude = 0.25f;
    public float floatFrequency = 1f;
    public float rotationSpeed = 45f;

    private Vector3 startPos;
    private bool isActivated = false;

    private void Start()
    {
        gunManager = FindObjectOfType<GunManager>();
        if (itemToActivate != null)
        {
            startPos = transform.position;
        }
    }

    private void Update()
    {
        if (!isActivated && itemToActivate != null)
        {
            // Mouvement de haut en bas (sinus)
            Vector3 pos = startPos;
            pos.y += Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;
            transform.position = pos;

            // Rotation lente autour de l’axe Y
            transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime, Space.World);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isActivated)
        {
            if (itemToActivate != null)
            {
                itemToActivate.SetActive(true);
                gunManager.currentGun = itemToActivate.GetComponent<Gun>();

                isActivated = true;

                Debug.Log("Item activated: " + itemToActivate.name);
                Destroy(gameObject); 
            }
            else
            {
                Debug.LogWarning("No item assigned to activate.");
            }
        }
    }
}
