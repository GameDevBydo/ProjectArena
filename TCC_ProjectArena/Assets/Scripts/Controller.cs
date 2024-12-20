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
    [SerializeField] Animator animator;

    [HideInInspector]
    public bool online = false;

    [HideInInspector]
    public string playerTempName;
    
    

    void Awake()
    {
        if (instance == null)
        {
            Application.targetFrameRate = 60;
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
    public List<GameObject> cerca = new List<GameObject>();

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
        GameObject.FindWithTag("LobbyCamera").GetComponent<Camera>().depth = -3;
        if (deathScreen.activeSelf== false) Cursor.lockState = CursorLockMode.Locked;
        GameObject.FindWithTag("Player").GetComponent<Player>().RemoveLife(0);
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

    public void ConnectedClients(Player player)
    {
        if(IsHost)
        {
            switch(NetworkManager.ConnectedClients.Count)
            {
                case 2:
                    player.SetPosition(new Vector3(-0.33f, 2.78f, 2.88155f));
                    break;
                case 3:
                    player.SetPosition(new Vector3(-5.73f, 2.78f, 2.88155f));
                    break;
                default:
                    break;
            }
        }
        else
        Debug.Log("Não tem permissão pra isso");
    }

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
        Debug.Log("FUICHAMADO MESMO AQUI NO INICIO");
        slotsFilled = 0;
        UIController.instance.WriteOnHeader("PEDIDOS ABERTOS!", 15f);
        FillWave();
        StartCoroutine(UIController.instance.UpdateWaveFillTimer(15f));
        Invoke(nameof(CloseSlotsInWave), 14.9f);
        Invoke(nameof(StartCurrentWave), 15f);
        Debug.Log("Chamei aqui YEY");
    }

    public void FillWave()
    {
        enemiesInWave = new Enemy[(waveNumber - 1) * 3 + 5];
        for (int i = 0; i < enemiesInWave.Length; i++)
        {
            /*if (wavesInfos.waveBaseEnemy.Length >= waveNumber - 1)
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
            }*/

            if( waveNumber>3) enemiesInWave[i] = enemyPrefabList[Random.Range(0,enemyPrefabList.Length)].GetComponent<Enemy>();
            else enemiesInWave[i] = enemyPrefabList[Random.Range(0,enemyPrefabList.Length-1)].GetComponent<Enemy>();
            Debug.Log(enemiesInWave[i].enemyTypeID);
        }

        canFillWaveSlots = true;
    }

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
                if (slotsFilled > enemiesInWave.Length - 1)
                {
                    enemiesInWave[slotsFilled] = SpawnEnemy(user, enemyRequested);
                    slotsFilled++;
                }
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
            "pneu" => 0,
            "roboserra" => 1,
            "ratinho" => 2,
            "carrokabum" => 3,
            "ventilador" => 4,
            "bangbang" => 5,
            "tanque" => 6,
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
                
            }
        }
        //UIController.instance.WriteOnHeader("PEDIDOS FECHADOS!");
    }

    int enemiesAlive = 0;
    void StartCurrentWave() // Ativa os inimigos 1 a 1, e contabiliza quantos tem.
    {
        UIController.instance.WriteOnHeader("ONDA COMEÇOU!", Color.red, 5);
        //enemiesAlive = 0;
        Debug.Log("Inimigos no inicio da wave: " + enemiesAlive);
        foreach (Enemy e in enemiesInWave)
        {
            e.waveStart = true;
            enemiesAlive++;
        }
        Debug.Log("Inimigos vivos: " + enemiesAlive);
        foreach(GameObject c in cerca)
        {
            c.GetComponent<MovableObject>().MoveY(0.015f);
            c.GetComponent<MovableObject>().MoveZ(-0.00773f);
        }
    }

    public void EnemyKilled() // A ser puxado pelo Enemy, para quando ele morrer. Também checa se a wave foi limpa.
    {
        enemiesAlive--;
        Debug.Log("Inimigos vivos: " + enemiesAlive);
        if (enemiesAlive <= Mathf.RoundToInt(enemiesInWave.Length/3)) AudioControlador.instance.FadeInChant();
        if (enemiesAlive <= 0) WaveClearedRpc();
    }

    [Rpc(SendTo.Everyone)]
    void WaveClearedRpc() // Finaliza a wave e inicia a abertura de slots da próxima. 
    {
        if(IsHost)
        {
            waveNumber++;
            if(waveNumber%2==1)Invoke(nameof(OpenSlotsInWaveRpc), 5);
            else Invoke(nameof(OpenVoting), 5);
            foreach(GameObject c in cerca)
            {
                c.GetComponent<MovableObject>().MoveY(-0.015f);
                c.GetComponent<MovableObject>().MoveZ(0.00773f);
            }
        }
        UIController.instance.WriteOnHeader("ONDA " + (waveNumber-1) + " CONCLUÍDA!", Color.green, 5); 
        AudioControlador.instance.PlayCheer();

    }

    #endregion

    public GameObject[] enemyPrefabList;
    public Vector2[] enemySpawnPoints;
    public Enemy SpawnEnemy(string user, int enemyId)
    {
        int randomSpawnerId = Random.Range(0, 4);
        Vector2 randomPos = enemySpawnPoints[randomSpawnerId] + Random.insideUnitCircle * 10;
        Vector3 spawnPos = new Vector3(randomPos.x, 0, randomPos.y);
        GameObject e = Instantiate(enemyPrefabList[enemyId], spawnPos, Quaternion.identity).gameObject;
        e.name = e.name.Substring(0, e.name.Length-7);
        if (user != "AutoFill") e.name = e.name + " de " + user;
        NetworkObject eNetworkObject = e.GetComponent<NetworkObject>();
        eNetworkObject.Spawn();
        e.GetComponent<Enemy>().SetEnemyNameRpc(e.name);
        // if (user != "AutoFill") PrintSpawnAlert(user, enemyPrefabList[enemyId].name);
        if (user != "AutoFill") UIController.instance.PrintSpawnAlertRpc(user, enemyPrefabList[enemyId].name);
        return e.GetComponent<Enemy>();
    }

    #region Voting
    public List<string> votingName;
    bool canVote;
    int votingWinner = 0;


    public SO_VotingEffect[] effectsInfo;
    int effect1, effect2;
    [HideInInspector]
    public float votingValue1, votingValue2;
    int vote1 = 0, vote2 = 0;

    public void OpenVoting()
    {
        FillWave();
        CloseSlotsInWave();
        StartCoroutine(UIController.instance.UpdateWaveFillTimer(15f));
        votingValue1 = Random.Range(1, 5);
        votingValue2 = Random.Range(1, 5);
        effect1 = Random.Range(0,effectsInfo.Length-1);
        do
        {
            effect1 = Random.Range(0,effectsInfo.Length-1);
            effect2 = Random.Range(0,effectsInfo.Length-1);
        } 
        while(effect1==effect2);

        votingName = new List<string>();
        vote1 = 0;
        vote2 = 0;
        canVote = true;
            
        ShowVotingRpc(effect1, effect2, votingValue1, votingValue2);
    }

    [Rpc(SendTo.Everyone)]
    public void ShowVotingRpc(int effect1, int effect2, float votingValue1, float votingValue2)
    {
        UIController.instance.WriteOnHeader("VOTAÇÃO ABERTA!", 7.5f);
        UIController.instance.votingArea.SetActive(true);
        UIController.instance.FillOption1Info(effectsInfo[effect1], votingValue1*25);
        UIController.instance.FillOption2Info(effectsInfo[effect2], votingValue2*25);
        UIController.instance.UpdateVotingSlider(1,2);
        Invoke(nameof(CloseVoting), 15f);
    }
    public void ChatterVote(string name, string vote)
    {
        if(canVote)
        {
            if(!votingName.Contains(name))
            {
                votingName.Add(name);
                if(vote =="1") vote1++;
                else if(vote =="2") vote2++;
                UIController.instance.UpdateVotingSlider(vote1, vote1+vote2);
            }
        }
    }

    void CloseVoting()
    {
        canVote = false;
        Invoke(nameof(CountVotes), 0);
        Invoke(nameof(StartCurrentWave), 15);
    }

    void CountVotes()
    {
        SO_VotingEffect winnerEffect;
        float winnerValue = 1;
        votingWinner = 0;
        if(vote1!=vote2)
        {
            if(vote1>vote2) 
            {
                votingWinner = 1;
                winnerValue = votingValue1;
            }
            else if(vote2>vote1)
            {
                votingWinner = 2;
                winnerValue = votingValue2;
            }
        }
        else votingWinner = Random.Range(1,2);

        
        Debug.Log(votingWinner);

        if(votingWinner==1) winnerEffect = effectsInfo[effect1];
        else winnerEffect = effectsInfo[effect2];

        switch((int)winnerEffect.effectTarget)
        {
            case 0:
                foreach(Enemy e in enemiesInWave) VotingEffects.DecreaseDamageTaken(e, winnerValue);
                break;
            case 1:
                foreach(Enemy e in enemiesInWave) VotingEffects.IncreaseDamageTaken(e, winnerValue);
                break;
            case 2:
                foreach(Enemy e in enemiesInWave) VotingEffects.IncreaseDamageDealt(e, winnerValue);
                break;
            case 3:
                foreach(Enemy e in enemiesInWave) VotingEffects.DecreaseDamageDealt(e, winnerValue);
                break;
        }
        
        VoteResultsRpc(winnerEffect.effectName);
    }

    [Rpc(SendTo.Everyone)]
    public void VoteResultsRpc(string effectName)
    {
        UIController.instance.WriteOnHeader( "Efeito escolhido: \n" + effectName, 10f);
        UIController.instance.votingArea.SetActive(false);
    }

    #endregion

    public GameObject deathScreen;
    public void OpenDeathScreen()
    {
        deathScreen.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        GetComponent<AudioControlador>().MuteMainMixer();
    }



    public void OpenURL(string url)
    {
        Application.OpenURL(url);
    }

    public void LoadScene(int id)
    {
        SceneManager.LoadScene(id);
    }

    public GameObject CameraLook;
    public void SetCamera()
    {
        CameraLook = GameObject.Find("CameraBot");
        CameraLook.GetComponentInChildren<LookRotation>().SetTarget(GameObject.Find("HeadForCamera"));

    }
}
