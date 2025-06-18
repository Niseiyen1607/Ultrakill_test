using UnityEngine;
using System.Collections.Generic;
using SmallHedge.SoundManager;

public class RobotWalker : MonoBehaviour
{
    [Header("General Settings")]
    [SerializeField] private Transform body;
    [SerializeField] private float footSpacing;
    [SerializeField] private LayerMask terrain;

    [Header("Step Settings")]
    [SerializeField] private float stepDistance = 1f;
    [SerializeField] private float stepHeight = 0.5f;
    [SerializeField] private float speed = 5f;
    [SerializeField] private float raycastOffset = 1f;

    [Header("Step Timing")]
    [SerializeField] private float initialStepDelay = 0f;
    [SerializeField] private RobotWalker otherLeg;

    private Vector3 newPosition;
    private Vector3 currentPosition;
    private Vector3 oldPosition;
    private float lerp;
    private bool isStepping = false;
    private float stepTimer = 0f;

    void Start()
    {
        newPosition = body.position + (body.right * footSpacing);
        currentPosition = newPosition;
        oldPosition = newPosition;

        stepTimer = -initialStepDelay;
    }

    void Update()
    {
        transform.position = currentPosition;

        Vector3 rayOrigin = body.position + (body.right * footSpacing) + (Vector3.up * raycastOffset);
        Ray ray = new Ray(rayOrigin, Vector3.down);

        if (Physics.Raycast(ray, out RaycastHit info, 10f, terrain))
        {
            float distanceFromGroundPoint = Vector3.Distance(newPosition, info.point);

            if (!isStepping && (otherLeg == null || !otherLeg.isStepping) && distanceFromGroundPoint > stepDistance && stepTimer >= 0f)
            {
                lerp = 0;
                isStepping = true;
                oldPosition = currentPosition;
                newPosition = info.point;
                stepTimer = 0f;
            }
        }

        if (isStepping)
        {
            lerp += Time.deltaTime * speed;
            Vector3 footPos = Vector3.Lerp(oldPosition, newPosition, lerp);
            footPos.y += Mathf.Sin(lerp * Mathf.PI) * stepHeight;

            currentPosition = footPos;

            if (lerp >= 1f)
            {
                isStepping = false;
                oldPosition = newPosition;
            }
        }
        else
        {
            stepTimer += Time.deltaTime;
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(newPosition, 0.1f);

        if (body != null)
        {
            Gizmos.color = Color.green;
            Vector3 rayStart = body.position + (body.right * footSpacing) + (Vector3.up * raycastOffset);
            Gizmos.DrawLine(rayStart, rayStart + Vector3.down * 10f);
        }
    }

    public bool IsStepping()
    {
        return isStepping;
    }
}