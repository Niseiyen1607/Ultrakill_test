using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKFootSolver : MonoBehaviour
{
    [SerializeField] private Transform body;

    [SerializeField] private float footSpacing;

    [SerializeField] private LayerMask terrain;

    [SerializeField] private float stepDistance = 1f;

    [SerializeField] private float stepHeight = 0.5f;

    [SerializeField] private float speed = 5f;

    [SerializeField] private float raycastOffset = 1f;



    private Vector3 newPosition;

    private Vector3 currentPosition;

    private Vector3 oldPosition;

    private float lerp;



    private void Start()

    {

        // Initialize the foot position based on the body's position and foot spacing

        newPosition = body.position + (body.right * footSpacing);

        currentPosition = newPosition;

        oldPosition = newPosition;

    }



    void Update()

    {

        // Update the foot position

        transform.position = currentPosition;



        // Cast a ray downward to find the terrain

        Ray ray = new Ray(body.position + (body.right * footSpacing) + (Vector3.up * raycastOffset), Vector3.down);

        if (Physics.Raycast(ray, out RaycastHit info, 10f, terrain))

        {

            // If the foot is too far from the target position, start a new step

            if (Vector3.Distance(newPosition, info.point) > stepDistance)

            {

                lerp = 0;

                newPosition = info.point;

            }

        }



        // Animate the foot movement

        if (lerp < 1)

        {

            Vector3 footPosition = Vector3.Lerp(oldPosition, newPosition, lerp);

            footPosition.y += Mathf.Sin(lerp * Mathf.PI) * stepHeight;



            currentPosition = footPosition;

            lerp += Time.deltaTime * speed;

        }

        else

        {

            oldPosition = newPosition;

        }

    }



    private void OnDrawGizmos()

    {

        Gizmos.color = Color.red;

        Gizmos.DrawSphere(newPosition, 0.1f);



        // Draw the raycast

        if (body != null)

        {

            Gizmos.color = Color.green;

            Vector3 rayStart = body.position + (body.right * footSpacing) + (Vector3.up * raycastOffset);

            Gizmos.DrawLine(rayStart, rayStart + Vector3.down * 10f);

        }

    }
}