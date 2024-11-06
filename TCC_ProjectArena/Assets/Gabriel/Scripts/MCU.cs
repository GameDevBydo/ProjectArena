using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MCU : MonoBehaviour
{
    public Transform Target;
    public float Speed = 2f;
    public float Radius = 1f;
    public float Angle = 0f;

    // Update is called once per frame
    void Update()
    {
        float x = Target.position.x + Mathf.Cos(Angle) * Radius;
        //float y = Target.position.y;
        float z = Target.position.x + Mathf.Sin(Angle) * Radius;

        transform.position = new Vector3(x,transform.position.y, z);

        Angle += Speed * Time.deltaTime;

        transform.LookAt(Target);
    }
}
