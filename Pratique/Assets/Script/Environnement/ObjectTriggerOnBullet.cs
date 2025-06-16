using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ObjectTriggerOnBullet : MonoBehaviour
{
    [Header("Objet à déplacer")]
    [SerializeField] private Transform objectToMove;

    [Header("Mouvement")]
    [SerializeField] private Vector3 moveOffset = new Vector3(0, 1, 0);
    [SerializeField] private float moveDuration = 1f;
    [SerializeField] private Ease easing = Ease.OutBack;

    private Vector3 initialPosition;
    private bool hasMoved = false;

    private void Start()
    {
        if (objectToMove != null)
            initialPosition = objectToMove.position;
    }

    public void Activate()
    {
        if (!hasMoved && objectToMove != null)
        {
            hasMoved = true;
            objectToMove.DOMove(initialPosition + moveOffset, moveDuration).SetEase(easing);
        }
    }
}
