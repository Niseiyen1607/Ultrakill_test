using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateGun : MonoBehaviour
{
    [SerializeField] private Grappling grappling;

    private Quaternion desireRotation;
    private float rotation = 5f;

    private void Update()
    {
        if (!grappling.IsGrappling())
        {
            desireRotation = transform.parent.rotation;
        }
        else
        {
            desireRotation = Quaternion.LookRotation(grappling.GetGrapplePoint() - transform.position);
        }

        transform.rotation = Quaternion.Lerp(transform.rotation, desireRotation, rotation * Time.deltaTime);
    }
}
