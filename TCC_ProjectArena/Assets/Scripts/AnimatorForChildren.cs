using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

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

    public AudioSource sfxSource;

    public void PlaySFX()
    {
        sfxSource.Stop();
        sfxSource.pitch = Random.Range(0.8f, 1.2f);
        sfxSource.Play();
    }
}
