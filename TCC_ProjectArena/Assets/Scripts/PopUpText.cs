using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Unity.Netcode;

public class PopUpText : NetworkBehaviour 
{
    
    [SerializeField] private TextMeshProUGUI textObject;
    [HideInInspector] public float damageValue;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            textObject.text = damageValue.ToString();
            transform.localPosition += new Vector3(Random.Range(-0.2f, 0.2f), Random.Range(-0.2f, 0.2f), 0);
            textObject.fontSize = 0.5f+ damageValue/500;
            Invoke(nameof(Despawn), 3);
        }
    }

    void LateUpdate()
    {
        var cameraToLookAt = Camera.main;
        transform.LookAt(cameraToLookAt.transform);
        transform.rotation = Quaternion.LookRotation(cameraToLookAt.transform.forward);
    }

    void Despawn()
    {
        NetworkObject.Despawn();
    }
}