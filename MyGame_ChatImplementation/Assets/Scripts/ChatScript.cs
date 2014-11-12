using UnityEngine;
using System.Collections;
using Continental.Games;
using System.Collections.Generic;
using Continental.Shared;

public class ChatScript : MonoBehaviour
{
    private Rect windowRect = new Rect(200, 200, 300, 450);
    private string messageToSend = "";
    public static string messBox = "";
    public static string lastMessage = "";
    private string user = Login.myPlayer.username;
    private bool showChat = false;

    IEnumerable<CommandPair> commands;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab) == true)
        {
            showChat = true;
        }
        else if (Input.GetKeyDown(KeyCode.Escape) == true)
        {
            showChat = false;
        }

    }

    private void OnGUI()
    {
        if (showChat)
        {
            windowRect = GUI.Window(1, windowRect, windowFunc, "Chat");
        }
    }

    public UnityEngine.Vector2 scrollPosition;

    private void windowFunc(int id)
    {
        scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width(285), GUILayout.Height(395));
        GUILayout.Label(messBox);
        GUILayout.EndScrollView();
        GUILayout.BeginHorizontal();
        messageToSend = GUILayout.TextField(messageToSend);

        if (GUILayout.Button("Send", GUILayout.Width(75)))
        {
            Login.connection.Send(NetworkCommands.MESSAGE_SEND, new Message() { Msg = messageToSend });
            messageToSend = "";
        }

        scrollPosition.y = Mathf.Infinity;
        GUILayout.EndHorizontal();

        GUI.DragWindow(new Rect(0, 0, Screen.width, Screen.height));
    }
}
