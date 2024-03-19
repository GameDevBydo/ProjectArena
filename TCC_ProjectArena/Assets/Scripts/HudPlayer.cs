using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HudPlayer : MonoBehaviour
{
    [SerializeField] Image barLife;
    private void Start() {
        Controller.instance.hudPlayer= this;
    }
    public void SetValBarLife(float life, float maxlife)=> barLife.fillAmount = life / maxlife;
    
}
