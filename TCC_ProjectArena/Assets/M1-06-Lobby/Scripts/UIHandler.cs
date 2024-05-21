using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;

public class UIHandler : MonoBehaviour
{
    public GameObject mainPanel;
    public GameObject createLobbyPanel;
    public GameObject joinLobbyPanel;
    public GameObject listLobbiesPanel;

    public TMP_InputField mainPanelPlayerNameInput;

    public TMP_InputField lobbyCreateNameInput;
    public TMP_InputField lobbyGameModeInput;
    public TMP_InputField lobbyCreateMaxPlayersInput;
    public Toggle lobbyCreateIsPrivateToggle;

    public TMP_InputField availableSlotsInput;

    public TMP_InputField lobbyJoinCodeInput;


    public static UIHandler instance;

    private void Start()
    {
        instance = this;
    }




    /* ==============================================================
     * Main Panel Functions
     * ==============================================================
     */
    public void ToggleCreateLobbyPanel()
    {

        bool isActive = createLobbyPanel.activeSelf;

        createLobbyPanel.SetActive(!isActive);
        mainPanel.SetActive(false);
        
    }

    public void ToggleJoinLobbyPanel()
    {

        bool isActive = joinLobbyPanel.activeSelf;

        joinLobbyPanel.SetActive(!isActive);
        mainPanel.SetActive(false);
      
    }

    public void ToggleListLobbiesPanel()
    {

        bool isActive = listLobbiesPanel.activeSelf;

        listLobbiesPanel.SetActive(!isActive);
        mainPanel.SetActive(false);

    }

    public void ListAllLobbiesButton()
    {
        TesteConexao.instance.ListaLobbies();
    }

    /* ==============================================================
    * Create Lobby Panel Functions
    * ==============================================================
    */

    public void CreateLobbyButton()
    {
        string lobbyName = lobbyCreateNameInput.text;
        string gameMode = lobbyGameModeInput.text;
        int maxPlayers = Int32.Parse(lobbyCreateMaxPlayersInput.text);
        bool isPrivate = lobbyCreateIsPrivateToggle.isOn;

        TesteConexao.instance.CriaLobby(lobbyName, maxPlayers, isPrivate, gameMode);
    }

    
    public void BackMainPanelInCreatePanel()
    {
        createLobbyPanel.SetActive(false);
        mainPanel.SetActive(true);
    }

    /* ==============================================================
    * Join Lobby Panel Functions
    * ==============================================================
    */

    public void JoinLobbyByCodeButton()
    {
        string lobbyCode = lobbyJoinCodeInput.text;
        TesteConexao.instance.JoinLobbyByCode(lobbyCode);
    }

    public void QuickJoinLobbyButton()
    {
        TesteConexao.instance.QuickJoinLobby();
    }

    

    public void BackMainPanelInJoinPanel()
    {
        joinLobbyPanel.SetActive(false);
        mainPanel.SetActive(true);
    }

    /* ==============================================================
    * List Lobbies Panel Functions
    * ==============================================================
    */

    public void FilterListLobbiesButton()
    {
        string availableSlots = availableSlotsInput.text;
        TesteConexao.instance.FiltraListaLobbies(availableSlots);
    }

    public void BackMainPanelInListPanel()
    {
        listLobbiesPanel.SetActive(false);
        mainPanel.SetActive(true);
    }


}
