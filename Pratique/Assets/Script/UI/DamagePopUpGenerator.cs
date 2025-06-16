using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DG.Tweening; 

public class DamagePopUpGenerator : MonoBehaviour
{
    public static DamagePopUpGenerator Instance;

    [SerializeField] public GameObject damagePopUpPrefab;

    [Header("DoTween Animation Settings")]
    [SerializeField] private float moveYAmount = 1.5f; 
    [SerializeField] private float duration = 1f; 
    [SerializeField] private Ease easeType = Ease.OutQuad; 
    [SerializeField] private float fadeOutDelay = 0.5f; 

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void CreatePopUp(Vector3 position, string damageAmount, Color color)
    {
        GameObject popUp = Instantiate(damagePopUpPrefab, position, Quaternion.identity);
        TextMeshProUGUI tmpText = popUp.transform.GetChild(0).GetComponent<TextMeshProUGUI>();

        tmpText.text = damageAmount;
        tmpText.faceColor = color;

        // Rotation shake
        popUp.transform.DOShakeRotation(
            duration: duration,
            strength: new Vector3(0, 0, 90),
            vibrato: 20,
            randomness: 90,
            fadeOut: false
        );

        // Zoom sequence
        Sequence zoomSeq = DOTween.Sequence();
        zoomSeq.Append(popUp.transform.DOScale(1.5f, 0.1f).SetEase(Ease.OutBack));
        zoomSeq.Append(popUp.transform.DOScale(0.9f, 0.05f).SetEase(Ease.InOutSine));
        zoomSeq.Append(popUp.transform.DOScale(1f, 0.05f).SetEase(Ease.OutSine));

        // Use moveYAmount and easeType
        popUp.transform.DOMoveY(popUp.transform.position.y + moveYAmount, duration).SetEase(easeType);

        // Fade out
        float fadeDuration = 0.3f;
        tmpText.DOFade(0f, fadeDuration).SetEase(easeType).SetDelay(fadeOutDelay);

        DOVirtual.DelayedCall(duration, () =>
        {
            Destroy(popUp);
        });
    }
}