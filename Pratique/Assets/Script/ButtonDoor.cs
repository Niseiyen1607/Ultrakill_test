using UnityEngine;
using DG.Tweening;

public class ButtonDoor : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform door;
    [SerializeField] private Vector3 openOffset = new Vector3(0, 3, 0);
    [SerializeField] private float doorDuration = 1f;

    [Header("Button Animation")]
    [SerializeField] private Transform buttonVisual; // l'objet visuel du bouton
    [SerializeField] private float buttonPressDistance = 0.05f;
    [SerializeField] private float buttonPressDuration = 0.1f;

    [Header("Interaction")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    private Vector3 doorClosedPos;
    private bool isOpen = false;
    private bool playerNearby = false;

    private void Start()
    {
        doorClosedPos = door.position;
    }

    private void Update()
    {
        if (playerNearby && Input.GetKeyDown(interactKey))
        {
            ToggleDoor();
            PressButtonVisual();
        }
    }

    private void ToggleDoor()
    {
        if (isOpen)
            door.DOMove(doorClosedPos, doorDuration).SetEase(Ease.InQuad);
        else
            door.DOMove(doorClosedPos + openOffset, doorDuration).SetEase(Ease.OutQuad);

        isOpen = !isOpen;
    }

    private void PressButtonVisual()
    {
        if (buttonVisual == null) return;

        Vector3 startPos = buttonVisual.localPosition;
        Vector3 pressedPos = startPos - new Vector3(0, buttonPressDistance, 0);

        buttonVisual.DOLocalMove(pressedPos, buttonPressDuration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                buttonVisual.DOLocalMove(startPos, buttonPressDuration).SetEase(Ease.InQuad);
            });
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            playerNearby = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            playerNearby = false;
        }
    }
}
