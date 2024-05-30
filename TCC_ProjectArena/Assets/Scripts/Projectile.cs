using UnityEngine;

public class Projectile : MonoBehaviour
{
    public enum projectileType
    {
        BOLT,
        SCREW,
        BOMB
    }

    public projectileType pType;

    [HideInInspector]
    public int damage, pushback;
    public SO_AttackProperty[] projectileProperties;

    public string ownerName;

    void Start()
    {
        damage = projectileProperties[(int)pType].GetAttack();
        pushback = projectileProperties[(int)pType].GetKnockback();
        float power = 5;

        power = pType == projectileType.BOMB ? 5 : 15; // ESSA LINHA TA ESTRANHA CONFERIR DEPOIS **********
        Debug.Log(power);
        GetComponent<Rigidbody>().AddForce(transform.forward * power, ForceMode.Impulse);
        if(pType == projectileType.BOMB) Invoke("Explode", 2);
        else Destroy(gameObject, 2);
    }

    void OnTriggerEnter(Collider col)
    {
        //if(col.tag == "Enemy")
        //{
        //    if(pType != projectileType.BOMB)Destroy(gameObject);
        //}
        //if(col.tag == "Ground")
        //{
        //    //transform.GetComponent<Collider>().enabled = false;
        //}
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