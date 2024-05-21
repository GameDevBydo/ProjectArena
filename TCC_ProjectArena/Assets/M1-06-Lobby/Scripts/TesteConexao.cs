using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class TesteConexao : MonoBehaviour
{
    private Lobby hostLobby;
    private Lobby joinedLobby;
    private float temporizadorAtivacaoLobby;

    public static TesteConexao instance;


    private async void Start()
    {
        instance = this;

        // Dispara uma rotina de inicializa��o da API
        await UnityServices.InitializeAsync();

        // Escuta pelo evento de login do jogador
        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Conectado como: " + AuthenticationService.Instance.PlayerId);
        };

        // Dispara uma rotina de login do usuario (de forma an�nima)
        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        string nomePlayer = "Fulaninho" + Random.Range(1, 100);
        UIHandler.instance.mainPanelPlayerNameInput.text = nomePlayer;

    }


    private void Update()
    {
        ManterLobbyAtivo();       

    }

    /* Mantem o Lobby atual sempre ativo --> evita o timeout de 3 segundos */
    /* A cada 15 segundos, manda um ping para o server da Unity  */
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

    public async void CriaLobby(string lobbyName, int maxPlayers, bool isPrivate, string gameMode)
    {
        try
        {
            string playerName = UIHandler.instance.mainPanelPlayerNameInput.text;

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
                   {"GameMode", new DataObject(DataObject.VisibilityOptions.Public, gameMode,
                   DataObject.IndexOptions.S1)},
                }
            };

            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, createLobbyOptions);
     
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
                $"N�mero de players: {lobby.MaxPlayers}\tID: {lobby.Id}\tToken: {lobby.LobbyCode}\tPrivate? {lobby.IsPrivate}");
        
        }catch(LobbyServiceException e)
        {
            Debug.Log(e.Message);
        }

    }
    public async void ListaLobbies(QueryLobbiesOptions queryLobbiesOptions=null)
    {
        try
        {
            QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync(queryLobbiesOptions);

            Debug.Log($"Encontrei {queryResponse.Results.Count} lobbie(s):");
            int i = 1;
            foreach(Lobby lobby in queryResponse.Results)
            {
                Debug.Log($"\tLobby[{i}]: {lobby.Name}\tLobby Code: {lobby.Data["LobbyCode"].Value}");
                MostraInformacoesPlayers(lobby);
                i++;
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e.Message);
        }

    }

    public void FiltraListaLobbies(string availableSlots) 
    {
        try
        {
            QueryLobbiesOptions queryLobbiesOptions = new QueryLobbiesOptions
            {
                // Retorna os primeiros 25 lobbies encontrados com o filtro
                Count = 25,
                Filters = new List<QueryFilter>
                {
                    // Filtra lobbies com AvailableSlots > availableSlots (greater than)
                    new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, availableSlots, QueryFilter.OpOptions.GT),
                    //new QueryFilter(QueryFilter.FieldOptions.S1, "<Modo de Jogo>", QueryFilter.OpOptions.EQ)
                },
                Order = new List<QueryOrder>
                {
                    // Ordena de forma decrescente pela data de Cria��o
                    new QueryOrder(false, QueryOrder.FieldOptions.Created)
                }
            };

            ListaLobbies(queryLobbiesOptions);

        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e.Message);
        }

    }

    public async void JoinLobbyByCode(string lobbyCode)
    {
        try
        {
            string playerName = UIHandler.instance.mainPanelPlayerNameInput.text;
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

            MostraInformacoesPlayers(joinedLobby);

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
            string playerName = UIHandler.instance.mainPanelPlayerNameInput.text;
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

            MostraInformacoesPlayers(joinedLobby);

        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e.Message);
        }
    }

    private void MostraInformacoesPlayers(Lobby lobby)
    {
        Debug.Log($"Mostrando informacoes dos jogadores do lobby {lobby.Name}");
        foreach(Unity.Services.Lobbies.Models.Player player in lobby.Players)
        {
             Debug.Log($"\tNome: {player.Data["PlayerName"].Value}\tID: {player.Id}");
            // Debug.Log($"\tID: {player.Id}");
        }
    
    }

    /* Operacoes com Relay */
    private async void CriaRelay(int numberOfConnections)
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(numberOfConnections);

            string joinCodeRelay = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);


            Debug.Log($"Criei uma nova aloca��o de relay com o c�digo: {joinCodeRelay}");

        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
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
        // Tipo da Conex�o: DTLS --> Conex�o Segura
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(joinAllocation, "dtls"));
        return !string.IsNullOrEmpty(joinCode) && NetworkManager.Singleton.StartClient();
    }


}
