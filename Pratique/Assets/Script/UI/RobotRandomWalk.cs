using System.Collections;
using UnityEngine;

public class RobotRandomWalk : MonoBehaviour
{
    public float minMoveSpeed = 0.8f;
    public float maxMoveSpeed = 2f;
    public float directionChangeFrequency = 1.5f;
    public float rotationSmoothness = 2f;

    [Header("Obstacle Detection")]
    public float obstacleDetectionDistance = 1f;
    public LayerMask obstacleLayerMask; // Assure-toi de définir les bons layers dans l'inspecteur

    private Vector3 targetDirection;
    private float currentSpeed;
    private bool isAvoidingObstacle = false;

    private void Start()
    {
        StartCoroutine(RandomizeMovement());
    }

    private void Update()
    {
        if (IsObstacleAhead())
        {
            // Stop and pick new direction if obstacle ahead
            if (!isAvoidingObstacle)
            {
                StartCoroutine(AvoidObstacleRoutine());
            }
            return; // ne bouge pas tant que nouvelle direction pas choisie
        }

        // Smooth movement
        transform.position += targetDirection * currentSpeed * Time.deltaTime;

        // Smooth rotation
        if (targetDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSmoothness);
        }
    }

    IEnumerator RandomizeMovement()
    {
        while (true)
        {
            ChooseRandomDirection();

            // Random speed
            currentSpeed = Random.Range(minMoveSpeed, maxMoveSpeed);

            // Petite pause ?
            if (Random.value < 0.2f)
            {
                currentSpeed = 0f;
                yield return new WaitForSeconds(Random.Range(0.5f, 1.2f));
            }

            yield return new WaitForSeconds(Random.Range(directionChangeFrequency * 0.8f, directionChangeFrequency * 1.5f));
        }
    }

    void ChooseRandomDirection()
    {
        float angle = Random.Range(0f, 360f);
        targetDirection = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)).normalized;
    }

    bool IsObstacleAhead()
    {
        return Physics.Raycast(transform.position + Vector3.up * 0.5f, transform.forward, obstacleDetectionDistance, obstacleLayerMask);
    }

    IEnumerator AvoidObstacleRoutine()
    {
        isAvoidingObstacle = true;
        yield return null;

        // Pick a new direction that is not facing an obstacle
        for (int i = 0; i < 10; i++)
        {
            ChooseRandomDirection();
            if (!IsObstacleAhead()) break;
        }

        isAvoidingObstacle = false;
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position + Vector3.up * 0.5f, transform.forward * obstacleDetectionDistance);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position + Vector3.up * 0.5f, obstacleDetectionDistance);
    }
}
