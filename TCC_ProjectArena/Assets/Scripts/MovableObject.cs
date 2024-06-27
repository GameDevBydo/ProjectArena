using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovableObject : MonoBehaviour
{   
    Vector3 startPoint;
    float endPointX, endPointY, endPointZ;
    float time;
    public float duration;
    bool running = false; 

    void Start()
    {
        startPoint = transform.localPosition;    
        endPointX = startPoint.x;
        endPointY = startPoint.y;
        endPointZ = startPoint.z;
        Controller.instance.cerca = this.gameObject;
    }
    public void MoveX(float disX)
    {
        startPoint = transform.localPosition;    
        endPointX = startPoint.x + disX;
        time = 0;
        running = true;
    }
    public void MoveY(float disY)
    {
        startPoint = transform.localPosition;    
        endPointY = startPoint.y + disY;
        time = 0;
        running = true;
    }
    public void MoveZ(float disZ)
    {
        startPoint = transform.localPosition;    
        endPointZ = startPoint.z + disZ;
        time = 0;
        running = true;
    }

    void FixedUpdate()
    {
        if(running)
        {
            time+=(1/duration)*0.02f;
            Mathf.Clamp(time, 0, 1);
            transform.localPosition = new Vector3(Mathf.SmoothStep(startPoint.x, endPointX, time), Mathf.SmoothStep(startPoint.y, endPointY, time), Mathf.SmoothStep(startPoint.z, endPointZ, time));
            if(time >= 1)
            {
                running = false;
            }
        }
    }


}
