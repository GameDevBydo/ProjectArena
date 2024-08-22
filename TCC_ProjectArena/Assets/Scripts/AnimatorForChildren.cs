using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorForChildren : MonoBehaviour
{
    public Player player;

    public void _StopAttacking()
    {
        player.StopAttacking();
    }

    public void _SpawnProjectile()
    {
        if(player.IsOwner) player.SpawnRegularProjectile_ServerRpc(0);
    }

    public void _SpawnBomb()
    {
        if(player.IsOwner) player.SpawnBombProjectile_ServerRpc();
    }
}
