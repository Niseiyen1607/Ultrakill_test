using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunManager : MonoBehaviour
{
    public Gun currentGun;

    void Update()
    {
        if (currentGun == null) return;

        // Tir principal (clic gauche)
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

        // Reload
        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("Reloading gun");
            currentGun.Reload();
        }
    }
}
