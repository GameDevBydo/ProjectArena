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
        player.SpawnRegularProjectile(0);
    }

    public void _SpawnBomb()
    {
        player.SpawnBombProjectile();
    }
}
