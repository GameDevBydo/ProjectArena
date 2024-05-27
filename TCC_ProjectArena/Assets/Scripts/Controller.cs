using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using System.Data;


public class Controller : NetworkBehaviour
{
    public static Controller instance;

    public NetworkVariable<bool> runStartedN = new();
    public HudPlayer hudPlayer;

    [HideInInspector]
    public bool online = false;

    [HideInInspector]
    public string playerTempName;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else Destroy(this.gameObject);
    }

    private async void Start()
    {
        NetworkManager.Singleton.ConnectionApprovalCallback = ApprovalCheck;
        instance = this;

        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Conectado como: " + AuthenticationService.Instance.PlayerId);
        };

        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        string nomePlayer = "Fulaninho" + Random.Range(1, 100);
    }

    void Update()
    {
        ManterLobbyAtivo();
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
        UIController.instance.ChangeUIArea(2);
        if (deathScreen.activeSelf== false) Cursor.lockState = CursorLockMode.Locked;

    }

    public void StartServer()
    {
        NetworkManager.Singleton.StartServer();
    }

    public string relayNumber;
    public async void PlayAsHost()
    {   
        CriaLobby(false); // Botando que não é private , MUDA DEPOIS
        LoadScene(1);
        await StartHostWithRelay();
        if(relayNumber != null)
        {
            UIController.instance.SetLobbyCode(relayNumber);
        }
    }
    [HideInInspector]
    public string lobbyCodeInput;
    public async void PlayAsClient()
    {
        LoadScene(1);
        await StartClientWithRelay(lobbyCodeInput);
    }

    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        if(NetworkManager.Singleton.ConnectedClients.Count>= 3)
        {
            response.Approved = false;
        }
        else
        {
            response.Approved = true;
            response.CreatePlayerObject = true;
        }
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

    #region Connection and Lobby
    private Lobby hostLobby;
    private Lobby joinedLobby;
    private float temporizadorAtivacaoLobby;

    private async void ManterLobbyAtivo()
    {
        if(hostLobby != null)
        {
            temporizadorAtivacaoLobby -= Time.deltaTime;

            if(temporizadorAtivacaoLobby < 0f)
            {
                temporizadorAtivacaoLobby = 15f;

                await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
            }
        }
    }

    public async void CriaLobby(bool isPrivate)
    {
        try
        {
            string playerName = playerTempName;

            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions
            {
                IsPrivate = isPrivate,
                Player = new Unity.Services.Lobbies.Models.Player
                {
                    Data = new Dictionary<string, PlayerDataObject>
                    {
                        {"PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, playerName)},
                    }
                },
                Data = new Dictionary<string, DataObject>
                {
                   {"GameMode", new DataObject(DataObject.VisibilityOptions.Public, "default",
                   DataObject.IndexOptions.S1)},
                }
            };

            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync("Batata", 3, createLobbyOptions);
     
            hostLobby = lobby;

            Lobby newLobby = await LobbyService.Instance.UpdateLobbyAsync(
                lobby.Id,
                new UpdateLobbyOptions
                {
                    Data = new Dictionary<string, DataObject>
                    {
                        {"LobbyCode", new DataObject(DataObject.VisibilityOptions.Public, lobby.LobbyCode)}
                    }
                });
                    
            Debug.Log($"O lobby {lobby.Name} foi criado por {playerName}\t" +
                $"Número de players: {lobby.Players.Count + "/" + lobby.MaxPlayers}\tID: {lobby.Id}\tToken: {lobby.LobbyCode}\tPrivate? {lobby.IsPrivate}");
        
        }catch(LobbyServiceException e)
        {
            Debug.Log(e.Message);
        }

    }

    public async void JoinLobbyByCode(string lobbyCode)
    {
        try
        {
            string playerName = playerTempName;
            JoinLobbyByCodeOptions options = new JoinLobbyByCodeOptions
            {
                Player = new Unity.Services.Lobbies.Models.Player
                {
                    Data = new Dictionary<string, PlayerDataObject>
                    {
                        {"PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, playerName)}
                    }
                }
            };

            Debug.Log($"Entrando no lobby {lobbyCode}");
            Lobby joinedLobby = await Lobbies.Instance.JoinLobbyByCodeAsync(lobbyCode, options);

        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e.Message);
        }
    }

    public async void QuickJoinLobby()
    {
        try
        {
            string playerName = playerTempName;
            JoinLobbyByCodeOptions options = new JoinLobbyByCodeOptions
            {
                Player = new Unity.Services.Lobbies.Models.Player
                {
                    Data = new Dictionary<string, PlayerDataObject>
                    {
                        {"PlayerName", 
                            new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, playerName)}
                    }
                }
            };

            Lobby joinedLobby = await Lobbies.Instance.QuickJoinLobbyAsync();

        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e.Message);
        }
    }

    private async Task<string> CriaRelay(int numberOfConnections)
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(numberOfConnections);
            string joinCodeRelay = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            Debug.Log($"Criei uma nova alocaço de relay com o código: {joinCodeRelay}");
            return joinCodeRelay;
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
            return null;
        }
    }

    private async void JoinRelay(string joinCodeRelay)
    {
        try
        {
            Debug.Log($"Entrando na alocação de relay com o código: {joinCodeRelay}");
            await RelayService.Instance.JoinAllocationAsync(joinCodeRelay);

        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async Task<string> StartHostWithRelay(int maxConnections = 5)
    {
        await UnityServices.InitializeAsync();
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
        Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections);
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(allocation, "dtls"));
        var joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
        relayNumber = joinCode;
        return NetworkManager.Singleton.StartHost() ? joinCode : null;
    }

    public async Task<bool> StartClientWithRelay(string joinCode)
    {
        await UnityServices.InitializeAsync();
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        var joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode: joinCode);
        // Tipo da Conexão: DTLS --> Conexão Segura
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(joinAllocation, "dtls"));
        return !string.IsNullOrEmpty(joinCode) && NetworkManager.Singleton.StartClient();
    }

    #endregion

    [Rpc(SendTo.Everyone)]
    public void OpenSlotsInWaveRpc() //Abre a fila para ser preenchida pelo chat. Automaticamente se preenche inteiramente com os inimigos recomendados da wave, ou da wave anterior.
    {
        slotsFilled = 0;
        // WriteOnHeader("PEDIDOS ABERTOS!", 8f);// referenciar o UI CONTROLLER
        UIController.instance.WriteOnHeader("PEDIDOS ABERTOS!", 8f);
        Debug.Log("Slots abertos.");
        enemiesInWave = new Enemy[(waveNumber - 1) * 5 + 10];
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
            "cart" => 3,
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
        Vector2 randomPos = Random.insideUnitCircle.normalized * 48;
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

    public List<string> votingName, votingValue;
    bool canVote;
    int votingWinner = 0;
    public void OpenVoting()
    {
        votingName = new List<string>();
        votingValue = new List<string>();
        canVote = true;
    }

    public void ChatterVote(string name, string vote)
    {
        if(canVote)
        {
            if(!votingName.Contains(name))
            {
                votingName.Add(name);
                votingValue.Add(vote);
            }
        }
    }

    void CloseVoting()
    {
        canVote = false;
        Invoke("CountVotes", 0);
    }

    void CountVotes()
    {
        votingWinner = 0;
        int vote1 = 0, vote2 = 0;
        foreach(string v in votingValue)
        {
            if(v == "1")
            {
                vote1++;
            }
            else if(v == "2")
            {
                vote2++;
            }
        }
        if(vote1!=vote2)
        {
            if(vote1>vote2) votingWinner = 1;
            else if(vote2>vote1) votingWinner = 2;
        }
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

}
