using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateItem : MonoBehaviour
{
    public enum ItemType
    {
        Revolver,
        Katana,
        Shotgun
    }

    [Header("Item Type")]
    public ItemType itemType;

    private GunManager gunManager;

    [Header("Oscillation Settings")]
    public float floatAmplitude = 0.25f;
    public float floatFrequency = 1f;
    public float rotationSpeed = 45f;

    private Vector3 startPos;
    private bool isActivated = false;

    private void Start()
    {
        gunManager = FindObjectOfType<GunManager>();
        startPos = transform.position;
    }

    private void Update()
    {
        if (!isActivated)
        {
            // Oscillation verticale
            Vector3 pos = startPos;
            pos.y += Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;
            transform.position = pos;

            // Rotation sur Y
            transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime, Space.World);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isActivated) return;

        if (other.CompareTag("Player"))
        {
            switch (itemType)
            {
                case ItemType.Revolver:
                    gunManager.ActivateRevolver();
                    break;
                case ItemType.Katana:
                    gunManager.ActivateKatana();
                    break;
                case ItemType.Shotgun:
                    gunManager.ActivateShotgun();
                    break;
            }

            isActivated = true;
            Destroy(gameObject);
        }
    }
}
