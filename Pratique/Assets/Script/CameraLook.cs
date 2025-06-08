using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraLook : MonoBehaviour
{
    public Transform player;

    private void Update()
    {
        transform.position = player.transform.position;
    }
}
