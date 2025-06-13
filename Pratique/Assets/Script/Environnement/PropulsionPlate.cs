using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropulsionPlate : MonoBehaviour
{
    public float force = 10f;
    private bool isOnPlate = false;

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isOnPlate = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && !isOnPlate)
        {
            isOnPlate = true;
        }
    }

    public float GetForce()
    {
        return isOnPlate ? force : 0f;
    }
}
