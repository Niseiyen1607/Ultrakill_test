using UnityEngine;
using DG.Tweening;

public class PressurePlate : MonoBehaviour
{
    [Header("Settings")]
    public Transform plateVisual; // Visuel qui descend
    public Vector3 pressedOffset = new Vector3(0, -0.1f, 0);
    public float pressSpeed = 5f;
    public LayerMask detectionLayer;

    [Header("Door")]
    public Transform doorTransform; // Transform au lieu de GameObject
    public Vector3 doorOpenOffset = new Vector3(0, 3f, 0); // Déplacement vers le haut
    public float doorMoveDuration = 0.75f;
    public Ease doorEase = Ease.OutCubic;

    public bool stayOpenWhilePressed = true;

    private Vector3 plateInitialPos;
    private Vector3 doorInitialPos;
    private int objectsOnPlate = 0;

    private void Start()
    {
        plateInitialPos = plateVisual.localPosition;
        if (doorTransform != null)
            doorInitialPos = doorTransform.position;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & detectionLayer) != 0)
        {
            objectsOnPlate++;
            if (objectsOnPlate == 1)
            {
                OpenDoor();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (((1 << other.gameObject.layer) & detectionLayer) != 0)
        {
            objectsOnPlate--;
            if (objectsOnPlate <= 0)
            {
                CloseDoor();
            }
        }
    }

    private void Update()
    {
        Vector3 targetPosition = objectsOnPlate > 0 ? plateInitialPos + pressedOffset : plateInitialPos;
        plateVisual.localPosition = Vector3.Lerp(plateVisual.localPosition, targetPosition, Time.deltaTime * pressSpeed);
    }

    private void OpenDoor()
    {
        if (doorTransform != null)
        {
            doorTransform.DOMove(doorInitialPos + doorOpenOffset, doorMoveDuration).SetEase(doorEase);
        }
    }

    private void CloseDoor()
    {
        if (stayOpenWhilePressed && doorTransform != null)
        {
            doorTransform.DOMove(doorInitialPos, doorMoveDuration).SetEase(doorEase);
        }
    }
}
