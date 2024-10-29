using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using TMPro;

public class UIController : NetworkBehaviour
{
    public void Update()
    {
        if (Input.anyKeyDown)
        {
            StartGame();
        }
    }
     [SerializeField]  Animator animCamera;
    [SerializeField] Animator animDor;
    [SerializeField] TMP_Text  txtStartGame;
    bool startibg = false;

    public void StartGame()
    {
        if (!startibg)
        {
            if (animCamera) animCamera.Play("Cam1");
            if (animDor)animDor.Play("portao");
            if (txtStartGame) txtStartGame.gameObject.SetActive(false);
            startibg = true;
        }
            
      
    }

    public void ExitGame()
    {
        Application.Quit();
    }

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

    [Rpc(SendTo.Everyone)]
    public void PrintSpawnAlertRpc(string user, string enemyName)
    {
        Debug.Log("CADE OS BICHOOOO SENDO SPAWNADOOOOO");
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
    public GameObject header;

    public TextMeshProUGUI txtHeader;

   public void WriteOnHeader(string message, Color color, float duration = 3.0f)
    {
        header.gameObject.SetActive(true);
        txtHeader.text = message;
        txtHeader.color = color;
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

    public Sprite[] classSprites, barLifeSprites, classUltIconSprites, classLightIconSprites, classHeavyIconSprites;
    public GameObject[] classBarLife;
    public Image classIcon, barLifeIcon, classUltIcon, classUltLoad, classLight, classHeavy;
    public TextMeshProUGUI[] playerNick;
    

    public void ChangeClassIcons(int id)
    {
        ActiveClassBarLife(id);
        classIcon.sprite = classSprites[id];
        barLifeIcon.sprite = barLifeSprites[id];
        classUltIcon.sprite = classUltIconSprites[id];
        classLight.sprite = classLightIconSprites[id];
        classHeavy.sprite = classHeavyIconSprites[id];;
    }
    public void ActiveClassBarLife(int id)
    {
        for (int i = 0; i < classBarLife.Length; i++)
        {
            classBarLife[i].gameObject.SetActive(false);
        }
        classBarLife[id].gameObject.SetActive(true);
    }

    public void SetPlayerName(string nick)
    {
        Controller.instance.playerTempName = nick;
        ChangeNameText();
    }

    public void ChangeNameText()
    {
        for(int i = 0;i < playerNick.Length; i++)
        {
            playerNick[i].text = Controller.instance.playerTempName;
        }
            
    }

    Color loadColor = new Vector4(0.6f, 0.6f, 0.6f, 0.5f);

    public void ChangeUltLoad(int value)
    {
        classUltLoad.fillAmount = value/100.0f;
        if(value == 100)
        {
            classUltIcon.gameObject.SetActive(true);
            classUltLoad.color = Color.white;
        }
        else
        {
            classUltIcon.gameObject.SetActive(false);
            classUltLoad.color = loadColor;
        }
    }

    #endregion

    #region Connections and Lobby

    [Header("Lobby")]
   public GameObject panelLobby;
    public Toggle lobbyCreateIsPrivateToggle;
    public TMP_InputField lobbyCreateNameInput, lobbyJoinCodeInput;
    public TextMeshProUGUI lobbyCodeText;
    public void ActivePanelLobby(bool active)
    {
        panelLobby.SetActive(active);
    }

    public void CreateLobbyButton()
    {
        string lobbyName = lobbyCreateNameInput.text;
        bool isPrivate = lobbyCreateIsPrivateToggle.isOn;

        //Controller.instance.CriaLobby(lobbyName, isPrivate);
    }

    public void JoinLobbyByCodeButton()
    {
        string lobbyCode = lobbyJoinCodeInput.text;
        TesteConexao.instance.JoinLobbyByCode(lobbyCode);
    }

    public void QuickJoinLobbyButton()
    {
        TesteConexao.instance.QuickJoinLobby();
    }

    public void SetLobbyCode(string lobbyCode)
    {
        lobbyCodeText.text = "Código do lobby: \n"+lobbyCode;
    }

    public void InputLobbyCode(string s)
    {
        Controller.instance.lobbyCodeInput = s;
    }

    public void CopyLobbyCode()
    {
        GUIUtility.systemCopyBuffer = Controller.instance.relayNumber;
    }

    public void SelectCharacter(int charID)
    {
        Player.instance.playerChar.Value = (Player.characterID)charID;
    }
    #endregion


    #region Voting Area
    public GameObject votingArea;
    public  Image votingBar;
    public TextMeshProUGUI option1Name, option1Info, option2Name, option2Info;
    public Image option1Image, option2Image;

    public void FillOption1Info(SO_VotingEffect effectInfo, float value)
    {
        option1Name.text = effectInfo.effectName;
        option1Image.sprite = effectInfo.effectSprite;
        option1Info.text = effectInfo.effectDescription + "\n" + value + "%";
    }
    public void FillOption2Info(SO_VotingEffect effectInfo, float value)
    {
        option2Name.text = effectInfo.effectName;
        option2Image.sprite = effectInfo.effectSprite;
        option2Info.text = effectInfo.effectDescription + "\n" + value  + "%";
    }

    public void UpdateVotingSlider(float mainVotes, float allVotes)
    {
        votingBar.fillAmount = mainVotes/allVotes;
    }
    
    #endregion

}