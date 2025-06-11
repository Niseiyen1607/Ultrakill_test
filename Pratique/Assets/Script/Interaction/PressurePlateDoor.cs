using UnityEngine;
using DG.Tweening;

public class PressurePlateDoor : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform door;
    [SerializeField] private Vector3 openPositionOffset;
    [SerializeField] private float doorOpenDuration = 1f;
    [SerializeField] private string triggeringTag = "Player";

    [Header("Pressure Plate Animation")]
    [SerializeField] private float plateDepressDistance = 0.1f;  // À quel point la plaque descend
    [SerializeField] private float plateDepressDuration = 0.2f;  // Vitesse d'animation

    private Vector3 doorClosedPos;
    private Vector3 plateStartPos;
    private bool isOpen = false;

    private void Start()
    {
        if (door != null)
            doorClosedPos = door.position;

        plateStartPos = transform.position; // position initiale de la plaque
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(triggeringTag) && !isOpen)
        {
            OpenDoor();
            DepressPlate();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(triggeringTag) && isOpen)
        {
            CloseDoor();
            ResetPlate();
        }
    }

    private void OpenDoor()
    {
        isOpen = true;
        door.DOMove(doorClosedPos + openPositionOffset, doorOpenDuration).SetEase(Ease.OutQuad);
    }

    private void CloseDoor()
    {
        isOpen = false;
        door.DOMove(doorClosedPos, doorOpenDuration).SetEase(Ease.InQuad);
    }

    private void DepressPlate()
    {
        transform.DOMoveY(plateStartPos.y - plateDepressDistance, plateDepressDuration).SetEase(Ease.OutQuad);
    }

    private void ResetPlate()
    {
        transform.DOMoveY(plateStartPos.y, plateDepressDuration).SetEase(Ease.InQuad);
    }
}
