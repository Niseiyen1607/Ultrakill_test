using UnityEngine;

public class WeaponRecoil : MonoBehaviour
{
    [Header("Recoil Settings")]
    [Tooltip("The Transform that will be moved and rotated for recoil. Typically the weapon's own Transform.")]
    public Transform recoilAnchor;
    [Range(0.01f, 2f)]
    public float recoilForce = 0.5f; // How far back/up the weapon moves initially
    [Range(0.1f, 50f)]
    public float recoilRotation = 5f; // How much the weapon rotates upwards initially
    [Range(1f, 20f)]
    public float recoilRecoverySpeed = 10f; // Speed at which the weapon returns to idle
    [Range(1f, 20f)]
    public float recoilSnappiness = 15f; // Speed at which the weapon "snaps" to its max recoil position

    [Tooltip("Local position offset for recoil (e.g., Z-negative for back, Y-positive for up).")]
    public Vector3 recoilOffset = new Vector3(0f, 0.05f, -0.1f);
    [Tooltip("Local rotation offset for recoil (e.g., X-negative for upwards rotation).")]
    public Vector3 recoilRotationOffset = new Vector3(-20f, 0f, 0f);

    [Header("Recoil Limits")]
    [Tooltip("Max Z-axis movement for recoil (local space). Prevents weapon from going too far back.")]
    public float maxZRecoil = 0f;
    [Tooltip("Min Z-axis movement for recoil (local space). Prevents weapon from going too far forward.")]
    public float minZRecoil = -0.2f;
    [Tooltip("Max X-axis rotation for recoil (local space). Prevents weapon from rotating too far up.")]
    public float maxXRecoilRotation = 0f;
    [Tooltip("Min X-axis rotation for recoil (local space). Prevents weapon from rotating too far down.")]
    public float minXRecoilRotation = -50f;

    // Internal variables for recoil state
    private Vector3 currentRecoilPosition;
    private Vector3 currentRecoilRotation;
    private Vector3 initialLocalPosition;
    private Quaternion initialLocalRotation;

    void Awake()
    {
        if (recoilAnchor == null)
        {
            recoilAnchor = transform; // If not set, use the object this script is on
        }

        // Store the initial local position and rotation of the recoil anchor
        initialLocalPosition = recoilAnchor.localPosition;
        initialLocalRotation = recoilAnchor.localRotation;
    }

    void Update()
    {
        // Smoothly move and rotate the weapon towards its current recoil state
        // and then smoothly bring it back to its idle position
        recoilAnchor.localPosition = Vector3.Lerp(recoilAnchor.localPosition, initialLocalPosition + currentRecoilPosition, Time.deltaTime * recoilSnappiness);
        recoilAnchor.localRotation = Quaternion.Slerp(recoilAnchor.localRotation, initialLocalRotation * Quaternion.Euler(currentRecoilRotation), Time.deltaTime * recoilSnappiness);

        // Reduce the current recoil values over time (recovery)
        currentRecoilPosition = Vector3.Lerp(currentRecoilPosition, Vector3.zero, Time.deltaTime * recoilRecoverySpeed);
        currentRecoilRotation = Vector3.Lerp(currentRecoilRotation, Vector3.zero, Time.deltaTime * recoilRecoverySpeed);
    }

    /// <summary>
    /// Applies an instant recoil impulse to the weapon. Call this when the weapon fires.
    /// </summary>
    public void ApplyRecoil()
    {
        // Add the recoil impulse to the current recoil state
        currentRecoilPosition += recoilOffset * recoilForce;
        currentRecoilRotation += recoilRotationOffset * recoilRotation;

        // Clamp the recoil to prevent excessive movement
        currentRecoilPosition.z = Mathf.Clamp(currentRecoilPosition.z, minZRecoil, maxZRecoil);
        currentRecoilRotation.x = Mathf.Clamp(currentRecoilRotation.x, minXRecoilRotation, maxXRecoilRotation);
    }

    /// <summary>
    /// Resets the weapon recoil immediately to its initial position.
    /// Useful for reloading or switching weapons.
    /// </summary>
    public void ResetRecoil()
    {
        currentRecoilPosition = Vector3.zero;
        currentRecoilRotation = Vector3.zero;
        recoilAnchor.localPosition = initialLocalPosition;
        recoilAnchor.localRotation = initialLocalRotation;
    }
}