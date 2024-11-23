using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class LookRotation : MonoBehaviour
{
    public GameObject Player;
    Vector3 dir;
    public float speed;
    Vector3 rot;

    // Update is called once per frame
    private void Start()
    {
        
    }
    void Update()
    {
         rot = new Vector3(Player.transform.position.x, Player.transform.position.y, 0);
        transform.LookAt(Player.transform);
        //transform.rotation.SetEulerRotation(transform.rotation.x, transform.rotation.y, 0); //x,y,0 
        MoveRobot();
    }

    public void SetTarget(GameObject Target)
    {
        
        Player = Target;
    }

    void MoveRobot()
    {
        dir = new Vector3(Player.transform.position.x - transform.position.x, 0, Player.transform.position.z - transform.position.z) ;
        if(dir.magnitude > 17) transform.position += dir.normalized * speed * Time.deltaTime;


    }
}
