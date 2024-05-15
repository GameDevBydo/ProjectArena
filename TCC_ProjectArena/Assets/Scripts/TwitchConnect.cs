using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.IO;
using TMPro;

public class TwitchConnect : MonoBehaviour
{
    [HideInInspector] public bool connectionStarted = false;
    private Controller main;
    TcpClient twitchClient;
    StreamReader reader;
    StreamWriter writer;

    const string URL = "irc.chat.twitch.tv";
    const int PORT = 6667;

    [HideInInspector] public string user = "";
    [HideInInspector] public string OAuth = "";
    [HideInInspector] public string channelName = "";

    private void ConnectToTwitch()
    {
        twitchClient = new TcpClient(URL, PORT);
        reader = new StreamReader(twitchClient.GetStream());
        writer = new StreamWriter(twitchClient.GetStream());

        writer.WriteLine("PASS "+ OAuth);
        writer.WriteLine("NICK "+ user.ToLower());
        writer.WriteLine("USER " + user.ToLower() + " 8 * :" +user);
        writer.WriteLine("JOIN #" + channelName.ToLower());
        writer.Flush();


        if(twitchClient.Connected)
        {
            connectionStarted = true;
            Debug.Log("Connected to Twitch.");
        }
        else
        {
            Debug.Log("Failed to connect to Twitch.");
        }

    }

    private void Awake()
    {
        //ConnectToTwitch();
        main = Controller.instance;
    }

    public TextMeshProUGUI connectionFeedback;
    public void ConnectTwitchBT()
    {
        ConnectToTwitch();
        if(connectionStarted)
        {
            connectionFeedback.text = "Conectado.";
        }
        else
        {
            connectionFeedback.text = "Falha na conexão.";
        }
    }

    void Update()
    {
        if(connectionStarted)
        {
            if(!twitchClient.Connected || twitchClient == null) ConnectToTwitch();
            ReadChat();
        }
    }

    void ReadChat()
    {
        if(twitchClient.Available >0)
        {
            string message = reader.ReadLine();
            if(message.Contains("PRIVMSG"))
            {
                int splitPoint = message.IndexOf("!", 1);
                string chatName = message[1..splitPoint];

                splitPoint = message.IndexOf(":", 1);
                string chatMessage = message[(splitPoint + 1)..];

                if(chatMessage.Length > 5 && chatMessage[..5].ToLower() == "spawn")
                {
                    string monsterName = chatMessage.Replace(" ", "")[5..].ToLower();
                    main.FillSlotInWave(chatName, monsterName);
                }

                if(chatMessage.Length > 4 && chatMessage[..4].ToLower() == "vote")
                {
                    string votingValue = chatMessage.Replace(" ", "")[4..].ToLower();
                    if(votingValue == "1" || votingValue == "2") main.ChatterVote(chatName, votingValue);
                }
            }
        }
    }

    public void SetUserIF(string userIF)
    {
        user = userIF;
    }

    public void SetOAuthIF(string oauthIF)
    {
        OAuth = oauthIF;
    }

    public void SetChannelNameIF(string channelIF)
    {
        channelName = channelIF;
    }

    public void AutoTestConnectBT()
    {
        user = "thebydo";
        OAuth = "oauth:kb9trff316jzd1gas936aakuze56pp";
        channelName = "thebydo";
        ConnectToTwitch();
        //main.ipAddress = "26.180.39.220";
        //main.ChangeIPAdress();
    }
}