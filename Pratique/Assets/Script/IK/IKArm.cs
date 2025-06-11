using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKArm : MonoBehaviour
{
    Transform joueur;

    public Vector3 decalage = new Vector3(0, 1.0f, 0);

    private void Start()
    {
        joueur = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        if (joueur != null)
        {
            transform.position = joueur.position + decalage;
        }
    }
}
