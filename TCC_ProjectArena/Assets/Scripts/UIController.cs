using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIController : MonoBehaviour
{
    [HideInInspector]
    public UIController instance;

    public TextMeshProUGUI notifText;

    void Awake()
    {
        if(instance == null) instance = this;
    }

    #region Chat
    public void PrintSpawnNotification(string user, string enemyName)
    {   
        PrintNotification(user + " selecionou " + enemyName + " para a batalha.");
    }

    public void PrintLoginNotification(string user)
    {
        PrintNotification(user + " se juntou a sua equipe!");
    }

    public void PrintNotification(string message, float time = 10)
    {
        notifText.gameObject.SetActive(true);
        notifText.text += "\n" + message;
        StartCoroutine(HideNotification(time));
    }

    IEnumerator HideNotification(float time = 10)
    {
        yield return new WaitForSeconds(time);
        notifText.gameObject.SetActive(false);
    }
    #endregion

    public TextMeshProUGUI alertsText;

    void WriteAlert(string message, Color color, float duration = 3.0f)
    {
        alertsText.gameObject.SetActive(true);
        alertsText.text = message;
        alertsText.color = color;
        StartCoroutine(CloseAlert(duration));
    }

    void WriteAlert(string message, float duration = 3.0f)
    {
        WriteAlert(message, Color.white, duration);
    }
    IEnumerator CloseAlert(float timer)
    {
        yield return new WaitForSeconds(timer);
        alertsText.gameObject.SetActive(false);
    }

    public Image waveFillTimer;
    IEnumerator UpdateWaveFillTimer(float duration)
    {
        float timer = duration;
        while(timer>0)
        {
            timer-=0.1f;
            waveFillTimer.fillAmount = timer/duration;
            yield return new WaitForSeconds(0.1f);
        }
    }
}
