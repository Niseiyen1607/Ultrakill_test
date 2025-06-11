using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIBillbording : MonoBehaviour
{
    private Camera cam;

    private void Awake()
    {
        cam = Camera.main;
    }

    private void LateUpdate()
    {
        transform.forward = cam.transform.forward;
    }
}
