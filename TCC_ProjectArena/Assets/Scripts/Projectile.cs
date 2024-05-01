using UnityEngine;

public class Projectile : MonoBehaviour
{
    public bool isBomb = false;

    public string ownerName;

    void Start()
    {
        float power;
        if(isBomb) power =5;
        else power = 10;
        GetComponent<Rigidbody>().AddForce(transform.forward * power, ForceMode.Impulse);
        if(isBomb) Invoke("Explode", 3);
        else Destroy(gameObject, 3);
    }

    void OnTriggerEnter(Collider col)
    {
        if(col.tag == "Enemy")
        {
            if(!isBomb)Destroy(gameObject);
        }
        if(col.tag == "Ground")
        {
            //transform.GetComponent<Collider>().enabled = false;
        }
    }

    public GameObject explosionEmpty;
    void Explode()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        explosionEmpty.SetActive(true);
        Destroy(gameObject, 1.3f);
    }

}