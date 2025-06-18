using System.Collections;
using UnityEngine;

public class RobotRandomWalk : MonoBehaviour
{
    [Header("Movement")]
    public float minMoveSpeed = 0.8f;
    public float maxMoveSpeed = 2f;
    public float directionChangeFrequency = 1.5f;
    public float rotationSmoothness = 2f;

    [Header("Obstacle Detection")]
    public float obstacleDetectionDistance = 1.5f;
    public LayerMask obstacleLayer;

    private Vector3 targetDirection;
    private float currentSpeed;

    private void Start()
    {
        StartCoroutine(RandomizeMovement());
    }

    private void Update()
    {
        // Check for obstacle directly ahead
        if (IsObstacleAhead())
        {
            PickNewDirection(); 
        }

        // Move and rotate smoothly
        transform.position += targetDirection * currentSpeed * Time.deltaTime;

        if (targetDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSmoothness);
        }
    }

    private bool IsObstacleAhead()
    {
        Vector3 origin = transform.position + Vector3.up * 0.5f; // slightly elevated to avoid ground hits
        return Physics.Raycast(origin, transform.forward, obstacleDetectionDistance, obstacleLayer);
    }

    private void PickNewDirection()
    {
        float angle = Random.Range(90f, 270f); 
        targetDirection = Quaternion.Euler(0, angle, 0) * transform.forward;
        targetDirection = targetDirection.normalized;
    }

    IEnumerator RandomizeMovement()
    {
        while (true)
        {
            if (!IsObstacleAhead())
            {
                float angle = Random.Range(0f, 360f);
                targetDirection = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)).normalized;
                currentSpeed = Random.Range(minMoveSpeed, maxMoveSpeed);

                if (Random.value < 0.2f)
                {
                    currentSpeed = 0f;
                    yield return new WaitForSeconds(Random.Range(0.5f, 1.2f));
                }
            }

            yield return new WaitForSeconds(Random.Range(directionChangeFrequency * 0.8f, directionChangeFrequency * 1.5f));
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Vector3 origin = transform.position + Vector3.up * 0.5f;
        Gizmos.DrawRay(origin, transform.forward * obstacleDetectionDistance);

        if (targetDirection != Vector3.zero)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, transform.position + targetDirection * 0.5f);
        }
    }
}
