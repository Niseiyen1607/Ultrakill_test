using UnityEngine;

public class RicochetPoint : MonoBehaviour
{
    public Transform nextTarget;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(transform.position, 0.2f);
    }
}
