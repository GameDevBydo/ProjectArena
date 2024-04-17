using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIController : MonoBehaviour
{
    [HideInInspector]
    public static UIController instance;

    public GameObject[] uiAreas;

    public void ChangeUIArea(int id)
    {
        if(uiAreas.Length >= id)
        {
            for(int i = 0; i < uiAreas.Length; i++)
            {
                if(i==id) uiAreas[i].SetActive(true);
                else uiAreas[i].SetActive(false);
            }
        }
    }

    public GameObject player2LifeBar, player3LifeBar;

    public void ActivatePlayer2LifeBar()
    {
        player2LifeBar.SetActive(true);
    }

    public void ActivatePlayer3LifeBar()
    {
        player3LifeBar.SetActive(true);
    }

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
    public void PrintSpawnAlert(string user, string enemyName)
    {
        alertsText.gameObject.SetActive(true);
        alertsText.text += "\n" + user + " selecionou " + enemyName + " para a batalha.";
        StartCoroutine(HideAlerts());
    }

    public void PrintLoginAlert(string user)
    {
        alertsText.gameObject.SetActive(true);
        alertsText.text += "\n" + user + " se juntou a sua equipe!";
        StartCoroutine(HideAlerts());
    }

    IEnumerator HideAlerts()
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
    
     public TextMeshProUGUI header;

   public void WriteOnHeader(string message, Color color, float duration = 3.0f)
    {
        header.gameObject.SetActive(true);
        header.text = message;
        header.color = color;
        StartCoroutine(CloseHeader(duration));
    }

   public void WriteOnHeader(string message, float duration = 3.0f)
    {
        WriteOnHeader(message, Color.white, duration);
    }
    IEnumerator CloseHeader(float timer) 
    {
        yield return new WaitForSeconds(timer);
        header.gameObject.SetActive(false);
    }



    #region User Names and Icons

    public Sprite[] classSprites;
    public Image classIcon;
    public TextMeshProUGUI playerNick;
    

    public void ChangeClassIcon(int id)
    {
        classIcon.sprite = classSprites[id];
    }

    public void SetPlayerName(string nick)
    {
        Player.instance.playerName = nick;
        ChangeNameText();
    }

    public void ChangeNameText()
    {
        playerNick.text = Player.instance.playerName;
    }

    #endregion

}
