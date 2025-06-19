using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [System.Serializable]
    public class WeaponUI
    {
        public string weaponName;
        public RectTransform uiElement;
        public CanvasGroup canvasGroup;
    }

    [SerializeField] private List<WeaponUI> weaponUIList;
    private Dictionary<string, WeaponUI> weaponUIDict;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        weaponUIDict = new Dictionary<string, WeaponUI>();
        foreach (var ui in weaponUIList)
        {
            weaponUIDict[ui.weaponName] = ui;
            SetInactiveState(ui);
        }
    }

    public void UpdateWeaponUI(List<string> unlockedWeapons, string currentWeapon)
    {
        foreach (var kvp in weaponUIDict)
        {
            if (unlockedWeapons.Contains(kvp.Key))
            {
                if (kvp.Key == currentWeapon)
                    SetEquippedState(kvp.Value);
                else
                    SetActiveState(kvp.Value);
            }
            else
            {
                SetInactiveState(kvp.Value);
            }
        }
    }

    private void SetEquippedState(WeaponUI ui)
    {
        ui.canvasGroup.DOFade(1f, 0.3f);
        ui.uiElement.DOScale(Vector3.one * 1.2f, 0.3f).SetEase(Ease.OutBack);
    }

    private void SetActiveState(WeaponUI ui)
    {
        ui.canvasGroup.DOFade(0.6f, 0.3f);
        ui.uiElement.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutQuad);
    }

    private void SetInactiveState(WeaponUI ui)
    {
        ui.canvasGroup.DOFade(0.2f, 0.3f);
        ui.uiElement.DOScale(Vector3.one * 0.8f, 0.3f).SetEase(Ease.InOutQuad);
    }
}
