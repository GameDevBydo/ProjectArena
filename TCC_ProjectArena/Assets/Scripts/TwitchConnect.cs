using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.IO;
using TMPro;

public class TwitchConnect : MonoBehaviour
{
    TcpClient twitchClient;
    StreamReader reader;
    StreamWriter writer;

    const string URL = "irc.chat.twitch.tv";
    const int PORT = 6667;

    string user = "thebydo";
    string OAuth = "oauth:kb9trff316jzd1gas936aakuze56pp";
    string channelName = "thebydo";

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

        Debug.Log("Connected to Twitch.");
    }

    private void Awake()
    {
        ConnectToTwitch();
    }

    void Update()
    {
        if(!twitchClient.Connected || twitchClient == null) ConnectToTwitch();

        ReadChat();
    }

    void ReadChat()
    {
        if(twitchClient.Available >0)
        {
            string message = reader.ReadLine();
            if(message.Contains("PRIVMSG"))
            {
                int splitPoint = message.IndexOf("!", 1);
                string chatName = message.Substring(1, splitPoint-1);

                splitPoint = message.IndexOf(":", 1);
                string chatMessage = message.Substring(splitPoint+1);

                PrintChatMessage(chatName, chatMessage);
                SpawnBall(chatName, int.Parse(chatMessage));
            }
        }
    }

    public TextMeshProUGUI chatLog;


    void PrintChatMessage(string user, string msg)
    {
        chatLog.text += "\n" + user +": " + msg;
    }

    public GameObject ball;
    void SpawnBall(string user, int msg)
    {
        GameObject a = Instantiate(ball, transform.position, transform.rotation).gameObject;
        a.transform.localScale *= msg;
        a.name = user + "'s ball";
        a.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = user;
    }



}
