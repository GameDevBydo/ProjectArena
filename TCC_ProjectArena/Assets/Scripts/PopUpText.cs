using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Unity.Netcode;
using System.Linq;

public class PopUpText : NetworkBehaviour 
{
    
    [SerializeField] private TextMeshProUGUI textObject;
    [HideInInspector] public NetworkVariable<float> damageValue = new();
    Camera playerCam;

    public override void OnNetworkSpawn()
    {
        var players = GameObject.FindObjectsOfType<Player>();
        playerCam = players.First(p=>p.IsOwner).playerOwnCamera;
        textObject.text = damageValue.Value.ToString();
        if(IsOwner) transform.localPosition += new Vector3(Random.Range(-0.2f, 0.2f), Random.Range(-0.2f, 0.2f), 0);
        textObject.fontSize = Mathf.Clamp(0.5f+ damageValue.Value/100, 0.5f, 1.5f);
        Invoke(nameof(Despawn), 3);
    }

    void LateUpdate()
    {
        transform.LookAt(playerCam.transform);
        transform.rotation = Quaternion.LookRotation(playerCam.transform.forward);
    }

    void Despawn()
    {
        if(IsOwner) NetworkObject.Despawn();
    }
}