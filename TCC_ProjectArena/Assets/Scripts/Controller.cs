using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using System.Data;


public class Controller : NetworkBehaviour
{
    public static Controller instance;

    public NetworkVariable<bool> runStartedN = new();
    public HudPlayer hudPlayer;

    [HideInInspector]
    public bool online = false;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else Destroy(this.gameObject);
    }

    public void IsOnline(bool b)
    {
        online = b;
    }


    // public TextMeshProUGUI alertsText; //VAI PRO UI CONTROLLER
    /* public void PrintSpawnAlert(string user, string enemyName) // VAI PRO UI CONTROLLER
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

 */
    #region Waves
    [HideInInspector] public int waveNumber = 1; // Número da wave atual (começa em 1, pois ia dar muita treta chamar a 1ª de 0).
    [HideInInspector] public Enemy[] enemiesInWave; //Inimigos presentes na wave atual para serem spawnados.
    public SO_WavesInfos wavesInfos; // Inimigos recomendados para serem spawnados, separados por wave.

    bool canFillWaveSlots = false;

    [Rpc(SendTo.Everyone)]
    public void StartRunRpc()
    {
        if (IsHost)
        {
            runStartedN.Value = true;
            OpenSlotsInWaveRpc();
        }
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void StartServer()
    {
        NetworkManager.Singleton.StartServer();
    }

    public void PlayAsHost()
    {
        NetworkManager.Singleton.StartHost();
    }

    public void PlayAsClient()
    {
        NetworkManager.Singleton.StartClient();
    }

    [HideInInspector]
    public string ipAddress;
    public void SetIPAddressIF(string s)
    {
        ipAddress = s;
    }

    public void ChangeIPAdress()
    {
        if (ipAddress == null || ipAddress.Length == 0)
        {
            ipAddress = "127.0.0.1";
        }
        UnityTransport transport = (UnityTransport)NetworkManager.Singleton.NetworkConfig.NetworkTransport;
        transport.ConnectionData.Address = ipAddress;
    }

    [Rpc(SendTo.Everyone)]
    public void OpenSlotsInWaveRpc() //Abre a fila para ser preenchida pelo chat. Automaticamente se preenche inteiramente com os inimigos recomendados da wave, ou da wave anterior.
    {
        // WriteOnHeader("PEDIDOS ABERTOS!", 8f);// referenciar o UI CONTROLLER
        UIController.instance.WriteOnHeader("PEDIDOS ABERTOS!", 8f);
        Debug.Log("Slots abertos.");
        enemiesInWave = new Enemy[(waveNumber - 1) * 3 + 10];
        for (int i = 0; i < enemiesInWave.Length; i++)
        {
            if (wavesInfos.waveBaseEnemy.Length >= waveNumber - 1)
            {
                if (wavesInfos.waveBaseEnemy[waveNumber - 1] != null) enemiesInWave[i] = wavesInfos.waveBaseEnemy[waveNumber - 1];
                else
                {
                    enemiesInWave[i] = wavesInfos.waveBaseEnemy[^1];
                    Debug.Log("Sem inimigos recomendados para essa onda, usando inimigos da onda anterior.");
                }
            }
            else
            {
                enemiesInWave[i] = wavesInfos.waveBaseEnemy[^1];
                Debug.Log("Onda não definida, usando inimigos da ultima onda.");
            }
        }
        canFillWaveSlots = true;

        /// StartCoroutine(UpdateWaveFillTimer()); // MUDAR PRO UI CONTROLLER
        StartCoroutine(UIController.instance.UpdateWaveFillTimer(10));
        Invoke(nameof(CloseSlotsInWave), 10f);

    }

    // public TextMeshProUGUI header; //vai pro ui controller

    /* void WriteOnHeader(string message, Color color, float duration = 3.0f) //vai pro ui controller
     {
         header.gameObject.SetActive(true);
         header.text = message;
         header.color = color;
         StartCoroutine(CloseHeader(duration));
     }
     */
    /* void WriteOnHeader(string message, float duration = 3.0f) // vai pro ui controller
     {
         WriteOnHeader(message, Color.white, duration);
     }
     IEnumerator CloseHeader(float timer) // vai pro ui controller
     {
         yield return new WaitForSeconds(timer);
         header.gameObject.SetActive(false);
     }


     public Image waveFillTimer; // vai pro ui controller
     IEnumerator UpdateWaveFillTimer() // vai pro ui controller
     {
         float timer = 10f;
         while(timer>0)
         {
             timer-=0.1f;
             waveFillTimer.fillAmount = timer/10f;
             yield return new WaitForSeconds(0.1f);
         }
    }*/

    int slotsFilled = 0; // Slots preenchidos por viewers.
    public void FillSlotInWave(string user, string enemyType)
    {
        // Função a ser chamada pelo TwitchConnect, recebe o nome de usuário e a mensagem. Se passar na checagem, adiciona a wave.
        // Ao lotar a wave, fecha ela e prepara para começar.
        if (canFillWaveSlots)
        {
            int enemyRequested = CheckEnemyRequest(enemyType);
            if (enemyRequested >= 0)
            {
                enemiesInWave[slotsFilled] = SpawnEnemy(user, enemyRequested);
                slotsFilled++;
                if (slotsFilled > enemiesInWave.Length - 1) CloseSlotsInWave();
            }
            else
            {
                Debug.Log("Inimigo selecionado não identificado.");
            }
        }
    }

    int CheckEnemyRequest(string enemyType) // Confere se o inimigo requisitado existe.
    {
        enemyType = enemyType.ToLower();
        var enemyId = enemyType switch
        {
            "bot" => 0,
            "bigbot" => 1,
            "rat" => 2,
            _ => -1,
        };
        return enemyId;
    }

    void CloseSlotsInWave() // Fecha os slots da wave, e prepara para iniciar os inimigos.
    {
        if (IsHost)
        {
            if (canFillWaveSlots)
            {
                canFillWaveSlots = false;
                if (slotsFilled < enemiesInWave.Length - 1)
                {
                    for (int i = slotsFilled; i < enemiesInWave.Length; i++)
                    {
                        enemiesInWave[i] = SpawnEnemy("AutoFill", enemiesInWave[i].enemyTypeID);
                    }
                }
                Invoke(nameof(StartCurrentWave), 5);
                // WriteOnHeader("PEDIDOS FECHADOS!"); // referenciar ui controller
                UIController.instance.WriteOnHeader("PEDIDOS FECHADOS!");
            }
        }
    }

    int enemiesAlive = 0;
    void StartCurrentWave() // Ativa os inimigos 1 a 1, e contabiliza quantos tem.
    {
        // WriteOnHeader("ONDA COMEÇOU!", Color.red, 5); // referenciar ui controller
        UIController.instance.WriteOnHeader("ONDA COMEÇOU!", Color.red, 5);
        enemiesAlive = 0;
        foreach (Enemy e in enemiesInWave)
        {
            e.waveStart = true;
            enemiesAlive++;
        }
        Debug.Log("Inimigos vivos: " + enemiesAlive);
    }

    public void EnemyKilled() // A ser puxado pelo Enemy, para quando ele morrer. Também checa se a wave foi limpa.
    {
        enemiesAlive--;
        Debug.Log("Inimigos vivos: " + enemiesAlive);
        if (enemiesAlive <= 0) WaveCleared();
    }

    void WaveCleared() // Finaliza a wave e inicia a abertura de slots da próxima. 
    {
        // WriteOnHeader("ONDA " + waveNumber + " CONCLUÍDA!", Color.green, 5); // referenciar ui controller
        UIController.instance.WriteOnHeader("ONDA " + waveNumber + " CONCLUÍDA!", Color.green, 5); // referenciar ui controller
        waveNumber++;
        Invoke(nameof(OpenSlotsInWaveRpc), 10);
    }

    #endregion


    public GameObject[] enemyPrefabList;
    public Enemy SpawnEnemy(string user, int enemyId)
    {
        Vector2 randomPos = Random.insideUnitCircle.normalized * 30;
        Vector3 spawnPos = new Vector3(randomPos.x, 0, randomPos.y);
        GameObject e = Instantiate(enemyPrefabList[enemyId], spawnPos, Quaternion.identity).gameObject;
        e.name = user + "'s " + e.name;
        NetworkObject eNetworkObject = e.GetComponent<NetworkObject>();
        eNetworkObject.Spawn();
        e.GetComponent<Enemy>().SetEnemyNameRpc(user);
        // if (user != "AutoFill") PrintSpawnAlert(user, enemyPrefabList[enemyId].name);
        if (user != "AutoFill") UIController.instance.PrintSpawnAlert(user, enemyPrefabList[enemyId].name);
        return e.GetComponent<Enemy>();
    }

    public void OpenVoting()
    {

    }

    public GameObject deathScreen;
    public void OpenDeathScreen()
    {
        deathScreen.SetActive(true);
    }



    public void OpenURL(string url)
    {
        Application.OpenURL(url);
    }

    public void LoadScene(int id)
    {
        SceneManager.LoadScene(id);
    }

    public void SelectCharacter(int charID)
    {
        Player.instance.playerChar.Value = (Player.characterID)charID;
    }

}
