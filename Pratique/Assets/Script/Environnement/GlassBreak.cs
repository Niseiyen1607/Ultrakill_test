using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlassBreak : MonoBehaviour
{
    [SerializeField] private GameObject glassBreakParticule;
    [SerializeField] private Transform point;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Instantiate(glassBreakParticule, point.transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }
}
