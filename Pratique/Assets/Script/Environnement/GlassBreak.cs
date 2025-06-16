using SmallHedge.SoundManager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlassBreak : MonoBehaviour
{
    [SerializeField] private GameObject glassBreakParticule;
    [SerializeField] private Transform point;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Bullet"))
        {
            SoundManager.PlaySound(SoundType.GLASSE_BREAK, null, 0.5f);
            Instantiate(glassBreakParticule, point.transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }
}
