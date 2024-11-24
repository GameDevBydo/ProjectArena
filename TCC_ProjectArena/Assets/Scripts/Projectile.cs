using UnityEngine;
using Unity.Netcode;

public class Projectile : NetworkBehaviour
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

        power = pType == projectileType.BOMB ? 8 : 25; // ESSA LINHA TA ESTRANHA CONFERIR DEPOIS **********
        GetComponent<Rigidbody>().AddForce(transform.forward * power, ForceMode.Impulse);
        if(pType == projectileType.BOMB) Invoke("Explode", 1f);
        else Destroy_ServerRpc(2);
    }

    [Rpc(SendTo.Server)]
    void Destroy_ServerRpc(float time)
    {
        Destroy(gameObject, time);
    }

    void OnTriggerEnter(Collider col)
    {
        
    }

    public GameObject explosionEmpty;
    void Explode()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        explosionEmpty.SetActive(true);
        Destroy_ServerRpc(0.45f);
    }

}