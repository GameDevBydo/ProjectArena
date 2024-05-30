
using UnityEngine;
[CreateAssetMenu(fileName = "AttackProperty", menuName = "TCC_ProjectArena/SO_AttackProperty")]
public class SO_AttackProperty :ScriptableObject
{
    [SerializeField] int attack, knockback;

    public int GetAttack() => attack;
    public int GetKnockback() => knockback;
}
