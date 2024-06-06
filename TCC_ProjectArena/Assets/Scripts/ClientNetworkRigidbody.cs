using Unity.Netcode.Components;
using UnityEngine;

namespace Unity.Multiplayer.Samples.Utilities.ClientAuthority
{

    [DisallowMultipleComponent]
    public class ClientNetworkRigidbody : NetworkRigidbody
    {        
        //protected override bool OnIsServerAuthoritative()
        //{
        //    return false;
        //}
    }
}