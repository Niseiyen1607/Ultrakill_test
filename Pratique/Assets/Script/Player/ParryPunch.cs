using System.Collections;
using UnityEngine;
using DG.Tweening; // N'oublie pas d'ajouter ça !

public class ParryPunch : MonoBehaviour
{
    public float punchRange = 3f;
    public float punchRadius = 1.5f;
    public LayerMask projectileMask;
    public Transform punchOrigin;

    [Header("Ralentissement")]
    public float slowMoTimeScale = 0.2f;
    public float slowMoDuration = 0.15f;

    [Header("Camera FX")]
    public Transform camTransform;
    public float shakeDuration = 0.2f;
    public float shakeStrength = 0.3f;
    public float zoomFOV = 30f;
    public float zoomDuration = 0.1f;
    public float zoomResetDuration = 0.2f;

    private Camera cam;
    private float originalFOV;

    private void Start()
    {
        cam = Camera.main;
        if (cam != null)
            originalFOV = cam.fieldOfView;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F)) // touche de parry
        {
            TryParry();
        }
    }

    void TryParry()
    {
        Collider[] hits = Physics.OverlapSphere(
            punchOrigin.position + punchOrigin.forward * punchRange * 0.5f,
            punchRadius,
            projectileMask);

        bool parryDone = false;

        foreach (Collider hit in hits)
        {
            Projectile proj = hit.GetComponent<Projectile>();
            if (proj && proj.canBeParried)
            {
                proj.Parry(Camera.main.transform.forward, 1.5f);
                parryDone = true;
                Debug.Log("Projectile parried!");
            }
        }

        if (parryDone)
        {
            DoSlowMotionWithFX();
        }
    }

    void DoSlowMotionWithFX()
    {
        // Time manipulation
        Time.timeScale = slowMoTimeScale;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;

        // Camera FX
        if (camTransform != null)
            camTransform.DOShakePosition(shakeDuration, shakeStrength, 10, 90f);

        if (cam != null)
        {
            cam.DOFieldOfView(zoomFOV, zoomDuration).OnComplete(() =>
            {
                cam.DOFieldOfView(originalFOV, zoomResetDuration);
            });
        }

        // Restore time after delay (realtime)
        StartCoroutine(ResetTimeAfterDelay(slowMoDuration));
    }

    IEnumerator ResetTimeAfterDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
    }

    private void OnDrawGizmosSelected()
    {
        if (punchOrigin == null) return;

        Gizmos.color = Color.cyan;
        Vector3 center = punchOrigin.position + punchOrigin.forward * punchRange * 0.5f;
        Gizmos.DrawWireSphere(center, punchRadius);
    }
}
