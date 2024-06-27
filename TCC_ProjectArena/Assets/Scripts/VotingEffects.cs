using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VotingEffects : MonoBehaviour
{
    #region Damage Dealt
    public static void IncreaseDamageDealt(Enemy e, float mod)
    {
        float value = 1+(mod/4);
        SetDamageDealtModifier(e, value);
    }
    public static void DecreaseDamageDealt(Enemy e, float mod)
    {
        float value = mod/4;
        SetDamageDealtModifier(e, value);
    }

    public static void SetDamageDealtModifier(Enemy e, float mod)
    {
        e.SetDamageDealtModifier(mod);
    }

    #endregion

    #region Damage Taken
    public static void IncreaseDamageTaken(Enemy e, float mod)
    {
        float value = 1+(mod/4);
        SetDamageTakenModifier(e, value);
    }
    public static void DecreaseDamageTaken(Enemy e, float mod)
    {
        float value = mod/4;
        SetDamageTakenModifier(e, value);
    }

    public static void SetDamageTakenModifier(Enemy e, float mod)
    {
        e.SetDamageTakenModifier(mod);
    }

    #endregion

    public static void HealAllTeam(int healAmmount)
    {
        Debug.Log(healAmmount);
    }

    public static void HealSpecificPlayer(int playerId, int healAmmount)
    {
        Debug.Log(playerId + ", " + healAmmount);
    }

    public static void RandomShoutOutToChat(string name, int healAmmount)
    {
        Debug.Log(name + ", " + healAmmount);
    }

    public static void CallAnyMethod(string name, params object[] args)
    {
        typeof(VotingEffects).GetMethod(name, System.Reflection.BindingFlags.Static).Invoke(null, args ?? System.Array.Empty<object>());
    }
    
}
