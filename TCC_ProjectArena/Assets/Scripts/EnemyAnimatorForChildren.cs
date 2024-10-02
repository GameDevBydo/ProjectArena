using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAnimatorForChildren : MonoBehaviour
{
    public Enemy enemy;

    public void _StopAttacking()
    {
        enemy.StopAttacking();
    }
}
