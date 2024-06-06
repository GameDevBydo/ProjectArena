using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SO_VotingEffect", menuName = "TCC_ProjectArena/SO_VotingEffect", order = 2)]
public class SO_VotingEffect : ScriptableObject
{
    public string effectName;
    public Sprite effectSprite;
    public string effectDescription;
    public string effectMethodName;
    public enum EffectTarget
    {
        NONE,
        RANDOMALLY,
        ALLALLIES,
        ALLENEMIES
    }
    public EffectTarget effectTarget;
}
