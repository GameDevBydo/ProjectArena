using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    [HideInInspector]public Rigidbody rb;
    [HideInInspector]public float damage;
    // Start is called before the first frame update
    void Awake()
    {
        rb=GetComponent<Rigidbody>();
    }

    void Start()
    {
        Destroy(gameObject, 1.5f);
    }
}