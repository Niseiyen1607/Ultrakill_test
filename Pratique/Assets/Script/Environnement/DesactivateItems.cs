using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DesactivateItems : MonoBehaviour
{
    public enum ItemType
    {
        Revolver,
        Katana,
        Shotgun,
        All,
    }

    [Header("Item Type")]
    public ItemType itemType;

    private GunManager gunManager;

    void Start()
    {
        gunManager = FindObjectOfType<GunManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            switch (itemType)
            {
                case ItemType.Revolver:
                    gunManager.DeactivateRevolver();
                    break;
                case ItemType.Katana:
                    gunManager.DeactivateKatana();
                    break;
                case ItemType.Shotgun:
                    gunManager.DeactivateShotgun();
                    break;
                case ItemType.All:
                    gunManager.DeactivateAllGuns();
                    break;
            }
            Destroy(gameObject);
        }
    }
}
