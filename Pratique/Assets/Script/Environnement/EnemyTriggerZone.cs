using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class EnemyTriggerZone : MonoBehaviour
{
    [Header("Spawning")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private Transform[] spawnPoints;

    [Header("Portes à animer")]
    [SerializeField] private Transform[] doorsToMove;
    [SerializeField] private Vector3 doorClosedOffset = new Vector3(0, 3, 0);
    [SerializeField] private float doorMoveDuration = 1f;
    [SerializeField] private Ease doorMoveEase = Ease.OutBack;

    [Header("Réglages")]
    [SerializeField] private string playerTag = "Player";

    private List<GameObject> spawnedEnemies = new List<GameObject>();
    private bool hasTriggered = false;
    private Vector3[] doorInitialPositions;

    private void Start()
    {
        doorInitialPositions = new Vector3[doorsToMove.Length];
        for (int i = 0; i < doorsToMove.Length; i++)
        {
            if (doorsToMove[i] != null)
                doorInitialPositions[i] = doorsToMove[i].position;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!hasTriggered && other.CompareTag(playerTag))
        {
            hasTriggered = true;
            SpawnEnemies();
            CloseDoors();
        }
    }

    private void Update()
    {
        if (!hasTriggered || spawnedEnemies.Count == 0) return;

        spawnedEnemies.RemoveAll(enemy => enemy == null);

        if (spawnedEnemies.Count == 0)
        {
            OpenDoors();
            hasTriggered = false; 
        }
    }

    private void SpawnEnemies()
    {
        foreach (var point in spawnPoints)
        {
            GameObject enemy = Instantiate(enemyPrefab, point.position, point.rotation);
            spawnedEnemies.Add(enemy);
        }
    }

    private void CloseDoors()
    {
        for (int i = 0; i < doorsToMove.Length; i++)
        {
            if (doorsToMove[i] != null)
            {
                Vector3 targetPos = doorInitialPositions[i] + doorClosedOffset;
                doorsToMove[i].DOMove(targetPos, doorMoveDuration).SetEase(doorMoveEase);
            }
        }
    }

    private void OpenDoors()
    {
        for (int i = 0; i < doorsToMove.Length; i++)
        {
            if (doorsToMove[i] != null)
            {
                doorsToMove[i].DOMove(doorInitialPositions[i], doorMoveDuration).SetEase(doorMoveEase);
            }
        }
    }
}
