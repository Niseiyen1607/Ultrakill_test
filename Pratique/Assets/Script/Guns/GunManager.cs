using System.Collections.Generic;
using UnityEngine;

public class GunManager : MonoBehaviour
{
    public Gun currentGun;

    [SerializeField] private GameObject shotgun;
    [SerializeField] private GameObject revolver;
    [SerializeField] private GameObject katana;

    private Dictionary<string, GameObject> allWeapons;
    private List<string> unlockedWeapons = new();
    private int currentWeaponIndex = 0;

    private void Start()
    {
        allWeapons = new Dictionary<string, GameObject>
        {
            { "Shotgun", shotgun },
            { "Revolver", revolver },
            { "Katana", katana }
        };

        DeactivateAllGuns();
    }

    private void Update()
    {
        if (currentGun != null)
        {
            if (currentGun.Automatic)
            {
                if (Input.GetMouseButton(0))
                    currentGun.TryShoot();
            }
            else
            {
                if (Input.GetMouseButtonDown(0))
                    currentGun.TryShoot();
            }

            if (Input.GetKeyDown(KeyCode.R))
                currentGun.Reload();
        }

        if (Input.GetKeyDown(KeyCode.Alpha1)) SwitchToWeaponIndex(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SwitchToWeaponIndex(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SwitchToWeaponIndex(2);

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0f) SwitchToNextWeapon();
        else if (scroll < 0f) SwitchToPreviousWeapon();
    }

    public void ActivateWeapon(string weaponName)
    {
        if (!allWeapons.ContainsKey(weaponName)) return;

        if (!unlockedWeapons.Contains(weaponName))
        {
            unlockedWeapons.Add(weaponName);
            Debug.Log(weaponName + " unlocked.");

            UIManager.Instance.UpdateWeaponUI(unlockedWeapons, weaponName);

        }

        SwitchToWeapon(weaponName);
    }

    private void SwitchToWeapon(string weaponName)
    {
        if (!unlockedWeapons.Contains(weaponName)) return;

        DeactivateAllGuns();

        GameObject weapon = allWeapons[weaponName];
        weapon.SetActive(true);
        currentGun = weapon.GetComponent<Gun>();
        currentWeaponIndex = unlockedWeapons.IndexOf(weaponName);

        UIManager.Instance.UpdateWeaponUI(unlockedWeapons, weaponName);

        Debug.Log(weaponName + " equipped.");
    }

    private void SwitchToWeaponIndex(int index)
    {
        if (index < 0 || index >= unlockedWeapons.Count) return;
        SwitchToWeapon(unlockedWeapons[index]);
    }

    private void SwitchToNextWeapon()
    {
        if (unlockedWeapons.Count <= 1) return;
        currentWeaponIndex = (currentWeaponIndex + 1) % unlockedWeapons.Count;
        SwitchToWeapon(unlockedWeapons[currentWeaponIndex]);
    }

    private void SwitchToPreviousWeapon()
    {
        if (unlockedWeapons.Count <= 1) return;
        currentWeaponIndex = (currentWeaponIndex - 1 + unlockedWeapons.Count) % unlockedWeapons.Count;
        SwitchToWeapon(unlockedWeapons[currentWeaponIndex]);
    }

    public void DeactivateAllGuns()
    {
        foreach (var weapon in allWeapons.Values)
            weapon?.SetActive(false);

        currentGun = null;

        UIManager.Instance.UpdateWeaponUI(unlockedWeapons, "");
    }

    public void DeactivateWeapon(string weaponName)
    {
        if (allWeapons.TryGetValue(weaponName, out GameObject weapon))
        {
            weapon.SetActive(false);
            if (currentGun == weapon.GetComponent<Gun>())
                currentGun = null;

            unlockedWeapons.Remove(weaponName);
            Debug.Log(weaponName + " deactivated.");

            UIManager.Instance.UpdateWeaponUI(unlockedWeapons, currentGun != null ? currentGun.name : "");
        }
    }

    public void ActivateRevolver() => ActivateWeapon("Revolver");
    public void ActivateShotgun() => ActivateWeapon("Shotgun");
    public void ActivateKatana() => ActivateWeapon("Katana");

    public void DeactivateRevolver() => DeactivateWeapon("Revolver");
    public void DeactivateShotgun() => DeactivateWeapon("Shotgun");
    public void DeactivateKatana() => DeactivateWeapon("Katana");
}
