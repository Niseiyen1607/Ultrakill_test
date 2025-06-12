using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunManager : MonoBehaviour
{
    public Gun currentGun;
    [SerializeField] private GameObject shotgun;
    [SerializeField] private GameObject revolver;
    [SerializeField] private GameObject katana;

    void Update()
    {
        if (currentGun == null) return;

        if (currentGun.Automatic)
        {
            if (Input.GetMouseButton(0))
            {
                Debug.Log("Automatic fire mode active");
                currentGun.TryShoot();
            }
        }
        else
        {
            if (Input.GetMouseButtonDown(0))
            {
                currentGun.TryShoot();
            }
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("Reloading gun");
            currentGun.Reload();
        }
    }

    public void ActivateKatana()
    {
        if (katana != null)
        {
            katana.SetActive(true);
            Debug.Log("Katana activated");
        }
        else
        {
            Debug.LogWarning("Katana GameObject is not assigned.");
        }
    }

    public void ActivateShotgun()
    {
        if (shotgun != null)
        {
            shotgun.SetActive(true);
            currentGun = shotgun.GetComponent<Gun>();
            Debug.Log("Shotgun activated");
        }
        else
        {
            Debug.LogWarning("Shotgun GameObject is not assigned.");
        }
    }

    public void ActivateRevolver()
    {
        if (revolver != null)
        {
            revolver.SetActive(true);
            currentGun = revolver.GetComponent<Gun>();
            Debug.Log("Revolver activated");
        }
        else
        {
            Debug.LogWarning("Revolver GameObject is not assigned.");
        }
    }
}
