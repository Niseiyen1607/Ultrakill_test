using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grappling : MonoBehaviour
{
    private LineRenderer lr;
    private Vector3 grapplePoint;
    [SerializeField] private LayerMask grappleMask;

    [SerializeField] private Transform gunTip, camera, player;

    [SerializeField] private float maxDistance = 100f;

    private SpringJoint springJoint;

    private void Awake()
    {
        lr = GetComponent<LineRenderer>();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            StartGrapple();
        }
        if (Input.GetMouseButtonUp(1))
        {
            StopGrapple();
        }
    }

    private void LateUpdate()
    {
        if (springJoint != null)
        {
            DrawRope();
        }
    }

    private void DrawRope()
    {
        if (!springJoint) return;

        lr.SetPosition(0, gunTip.position);
        lr.SetPosition(1, grapplePoint);
    }

    private void StartGrapple()
    {
        RaycastHit hit;
        if (Physics.Raycast(camera.position, camera.forward, out hit, maxDistance, grappleMask))
        {
            grapplePoint = hit.point;

            springJoint = player.gameObject.AddComponent<SpringJoint>();
            springJoint.autoConfigureConnectedAnchor = false;
            springJoint.connectedAnchor = grapplePoint;

            float distanceFromPoint = Vector3.Distance(player.position, grapplePoint);

            springJoint.maxDistance = distanceFromPoint * 0.8f;
            springJoint.minDistance = distanceFromPoint * 0.25f;

            springJoint.spring = 4.5f;
            springJoint.damper = 7f;
            springJoint.massScale = 4.5f;

            lr.positionCount = 2;
        }
    }

    private void StopGrapple()
    {
        if (springJoint != null)
        {
            Destroy(springJoint);
            lr.positionCount = 0;
        }
    }

    public bool IsGrappling()
    {
        return springJoint != null;
    }  
    
    public Vector3 GetGrapplePoint()
    {
        return grapplePoint;
    }
}
