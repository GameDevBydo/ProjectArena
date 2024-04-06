using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIController : MonoBehaviour
{
    [HideInInspector]
    public static UIController instance;

    public TextMeshProUGUI notifText;
    private void Start() {
        Debug.Log("ENTOU NESSE CONDIÇÃO DO UI CONTROLLER");
    }

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
    public void PrintSpawnAlert(string user, string enemyName) // VAI PRO UI CONTROLLER
    {
        alertsText.gameObject.SetActive(true);
        alertsText.text += "\n" + user + " selecionou " + enemyName + " para a batalha.";
        StartCoroutine(HideAlerts());
    }

    public void PrintLoginAlert(string user) // VAI PRO UI CONTROLLER
    {
        alertsText.gameObject.SetActive(true);
        alertsText.text += "\n" + user + " se juntou a sua equipe!";
        StartCoroutine(HideAlerts());
    }

    IEnumerator HideAlerts() // VAI PRO UI CONTROLLER
    {
        yield return new WaitForSeconds(5);
        alertsText.gameObject.SetActive(false);
    }

    public Image waveFillTimer;
   public IEnumerator UpdateWaveFillTimer(float duration)
    {
        float timer = duration;
        while(timer>0)
        {
            timer-=0.1f;
            waveFillTimer.fillAmount = timer/duration;
            yield return new WaitForSeconds(0.1f);
        }
    }
    
     public TextMeshProUGUI header; //vai pro ui controller

   public void WriteOnHeader(string message, Color color, float duration = 3.0f) //vai pro ui controller
    {
        header.gameObject.SetActive(true);
        header.text = message;
        header.color = color;
        StartCoroutine(CloseHeader(duration));
    }

   public void WriteOnHeader(string message, float duration = 3.0f) // vai pro ui controller
    {
        WriteOnHeader(message, Color.white, duration);
    }
    IEnumerator CloseHeader(float timer) // vai pro ui controller
    {
        yield return new WaitForSeconds(timer);
        header.gameObject.SetActive(false);
    }

}
