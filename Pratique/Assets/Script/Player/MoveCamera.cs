using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCamera : MonoBehaviour
{
    [SerializeField] Transform cameraPosition;
    [SerializeField] PlayerCam playerCam;

    void Update()
    {
        transform.position = cameraPosition.position;
    }
}
