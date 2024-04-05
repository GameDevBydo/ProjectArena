using UnityEngine;

public class Projectile : MonoBehaviour
{
    void Start()
    {
        Destroy(gameObject, 3);
    }

    void OnTriggerEnter(Collider col)
    {
        if(col.tag == "Enemy")
        {
            Destroy(gameObject);
        }
    }
}