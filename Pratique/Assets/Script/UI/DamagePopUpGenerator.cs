using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DG.Tweening; // Don't forget to add this using statement for DoTween!

public class DamagePopUpGenerator : MonoBehaviour
{
    public static DamagePopUpGenerator Instance;

    [SerializeField] public GameObject damagePopUpPrefab;

    [Header("DoTween Animation Settings")]
    [SerializeField] private float moveYAmount = 1.5f; // How high the pop-up moves
    [SerializeField] private float duration = 1f; // How long the animation lasts
    [SerializeField] private Ease easeType = Ease.OutQuad; // The easing function for movement
    [SerializeField] private float fadeOutDelay = 0.5f; // When the fade out starts

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

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P)) // For testing purposes
        {
            CreatePopUp(Vector3.one, Random.Range(0, 1000).ToString(), Color.yellow);
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

        float fadeDuration = 0.3f;
        tmpText.DOFade(0f, fadeDuration).SetEase(Ease.InQuad).SetDelay(duration - fadeDuration);

        DOVirtual.DelayedCall(duration, () =>
        {
            Destroy(popUp);
        });
    }

}