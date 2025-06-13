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

    public void DeactivateAllGuns()
    {
        if (shotgun != null) shotgun.SetActive(false);
        if (revolver != null) revolver.SetActive(false);
        if (katana != null) katana.SetActive(false);
        currentGun = null;
        Debug.Log("All guns deactivated");
    }

    public void DeactivateShotgun()
    {
        if (shotgun != null)
        {
            shotgun.SetActive(false);
            if (currentGun == shotgun.GetComponent<Gun>())
            {
                currentGun = null;
            }
            Debug.Log("Shotgun deactivated");
        }
        else
        {
            Debug.LogWarning("Shotgun GameObject is not assigned.");
        }
    }

    public void DeactivateRevolver()
    {
        if (revolver != null)
        {
            revolver.SetActive(false);
            if (currentGun == revolver.GetComponent<Gun>())
            {
                currentGun = null;
            }
            Debug.Log("Revolver deactivated");
        }
        else
        {
            Debug.LogWarning("Revolver GameObject is not assigned.");
        }
    }

    public void DeactivateKatana()
    {
        if (katana != null)
        {
            katana.SetActive(false);
            if (currentGun == katana.GetComponent<Gun>())
            {
                currentGun = null;
            }
            Debug.Log("Katana deactivated");
        }
        else
        {
            Debug.LogWarning("Katana GameObject is not assigned.");
        }
    }
}
