using System;
using UnityEngine;
using System.Collections.Generic;
using Continental;
using System.Linq;
using Continental.Games;
using Continental.Client;
using Continental.Shared;

/// <summary>
/// A Unity script for handling a Login scene. Also a quick player register is available.
/// </summary>
public class Login : MonoBehaviour
{
    /// <summary>
    /// GCC cloud or emulator server IP endpoint
    /// </summary>
    public string serverEndpoint = "127.0.0.1:13501";

    /// <summary>
    /// GCC cloud or emulator server port. Default port of Emulator is 13501
    /// </summary>
    //public int serverPort = 13501;//15000

    /// <summary>
    /// ID of your game in GCC cloud. 
    /// When running on GCC you will receive gameID upon game registration, for Emulator it is ignored.
    /// </summary>
    public string gameID = "abCdEfJKlMN";

    /// <summary>
    /// Last successfully logged in player. This field is accessible from any Unity script.
    /// </summary>
    public static Player myPlayer;
    /// <summary>
    /// list of username and password pairs. Used to store last logged in players for a single click login.
    /// </summary>
    SortedList<string, string> passwords;

    /// <summary>
    /// When network error occures a user friendly message is stored to this field.
    /// </summary>
    public static string lastError = "";
    public static string username;
    string password;
    public static ServerConnection connection;
    private bool handshakeDone = false;
    private ConnectionResponse response;
    public Version version;
    /// <summary>
    /// On network error the error message is stored and the game is re-directed to login scene.
    /// </summary>
    /// <param name="errorText"></param>
    public static void OnNetDisconnected(string errorText)
    {
        Login.lastError = errorText;
        Login.myPlayer = null;
        if (Application.loadedLevelName != "Login")
        {
            Application.LoadLevel("Login");
        }
    }

    void Start()
    {
        version = new Version(1, 0, 0, 0);
        InitPasswords();
    }

    /// <summary>
    /// Try to obtain a list of last used username and password pairs. If not found the list is initialized as empty.
    /// </summary>
    private void InitPasswords()
    {
        passwords = new SortedList<string, string>();
        string str = PlayerPrefs.GetString("passwords");
        if (!string.IsNullOrEmpty(str))
        {
            string[] vals = str.Split('\n');
            int i = 0;
            while (i < vals.Length)
            {
                passwords.Add(vals[i], vals[i + 1]);
                i += 2;
            }
            username = vals[0];
            password = vals[1];
        }
        else
        {
            username = "";
            password = "";
        }
    }


    /// <summary>
    /// On successful login if the username is not on the username/password pairs list it is stored in the PlayerPrefs Unity structure.
    /// </summary>
    void SavePasswords()
    {
        string result = "";
        bool first = true;
        foreach (KeyValuePair<string, string> pair in passwords)
        {
            if (first)
            {
                first = false;
            }
            else
            {
                result += "\n";
            }
            result += pair.Key;
            result += "\n";
            result += pair.Value;
        }

        PlayerPrefs.SetString("passwords", result);
        PlayerPrefs.Save();
    }

    void OnGUI()
    {
        GUIStyle errStyle = new GUIStyle();
        errStyle.normal.textColor = UnityEngine.Color.red;
        errStyle.fontSize = 15;
        GUIStyle styleHead = new GUIStyle();
        styleHead.fontSize = 25;
        styleHead.normal.textColor = new UnityEngine.Color(1f, .5f, .0f);
        GUI.skin.label.normal.textColor = new UnityEngine.Color(.3f, .3f, 1f);
        GUI.skin.label.fontSize = 16;
        GUI.skin.textField.fontSize = 15;
        GUI.skin.textField.normal.textColor = new UnityEngine.Color(0f, .9f, 0f);
        GUI.skin.textField.active.textColor = new UnityEngine.Color(0f, .9f, 0f);
        GUI.skin.textField.hover.textColor = new UnityEngine.Color(0f, .9f, 0f);
        GUI.skin.textField.focused.textColor = new UnityEngine.Color(0f, .9f, 0f);
        GUI.skin.textField.alignment = TextAnchor.MiddleLeft;
        GUI.skin.button.normal.textColor = new UnityEngine.Color(.4f, .6f, 1f);
        GUI.skin.button.hover.textColor = new UnityEngine.Color(.4f, .8f, 1f);

        GUI.Label(new Rect(10, 10, 120, 28), "LOGIN", styleHead);
        GUI.Label(new Rect(30, 50, 100, 28), "Username");
        GUI.Label(new Rect(30, 80, 100, 28), "Password");
        username = GUI.TextField(new Rect(150, 50, 120, 28), username);
        password = GUI.TextField(new Rect(150, 80, 120, 28), password);

        GUI.Label(new Rect(530, 50, 140, 28), "ServerEndpoint");
        GUI.Label(new Rect(530, 110, 100, 28), "GameID");
        serverEndpoint = GUI.TextField(new Rect(650, 50, 120, 28), serverEndpoint);
        gameID = GUI.TextField(new Rect(650, 110, 120, 28), gameID);

        GUI.Label(new Rect(130, 130, 150, 50), lastError, errStyle);

        if (GUI.Button(new Rect(10, 130, 100, 30), "Login"))
        {
            Connect(NetworkCommands.AUTHORIZE_REQUEST);
        }

        if (GUI.Button(new Rect(10, 180, 100, 30), "Register"))
        {
            Connect(NetworkCommands.REGISTER_REQUEST);
        }

        if (GUI.Button(new Rect(10, 230, 100, 30), "Login As Guest"))
        {
            Connect(NetworkCommands.AUTHORIZE_GUEST);
        }

        int yPos = 50;

        foreach (string key in passwords.Keys)
        {
            if (GUI.Button(new Rect(300, yPos, 100, 20), key))
            {
                username = key;
                password = passwords[key];
                Connect(NetworkCommands.AUTHORIZE_REQUEST);
            }
            yPos += 30;
        }

    }

    /// <summary>
    /// Clear last error message if any. Close network connection to server if open. Connect to game server and send authorize request.
    /// </summary>
    private void Connect(short context)
    {
        Debug.Log("ServerIPEndpoint: " + serverEndpoint.ToString());
        connection = new ServerConnection(serverEndpoint, gameID, version);
        connection.disconnectedCallback = OnNetDisconnected;
        lastError = "";
        try
        {
            connection.Disconnect(true);
            response = connection.Connect(context, new Player { username = username, password = password });
            handshakeDone = response.connectionSuccess;
            myPlayer = (Player)response.responseCommand;

            switch (response.responseContext)
            {
                case NetworkCommands.AUTHORIZE_RESPONSE_OK:
                    Debug.Log(myPlayer.playerId);
                    if (!passwords.ContainsKey(username))
                    {
                        passwords.Add(username, password);
                        SavePasswords();
                    }
                    Application.LoadLevel("Game");
                    break;
                case NetworkCommands.AUTHORIZE_RESPONSE_FAIL:
                    lastError = "Login failed!";
                    break;
                case NetworkCommands.REGISTER_RESPONSE_FAIL:
                    lastError = ((PlayerRegisterResponse)response.responseCommand).result.ToString();
                    break;
                case CommandCodes.HANDSHAKE_FAILURE:
                    lastError = ((ClosedConnectionInfo)response.responseCommand).reason.ToString();
                    break;
                case NetworkCommands.AUTHORIZE_GUEST_SUCESSFUL:
                    myPlayer = (Player)response.responseCommand;
                    Application.LoadLevel("Game");
                    break;
                default:
                    throw new Exception("Unsupported message code " + response.responseContext.ToString());
            }
            if (!handshakeDone)
            {
                Debug.Log(lastError);
                return;
            }
        }
        catch (Exception e)
        {
            Debug.Log(string.Format("Error occured during connection! Error: {0}", e.Message));
        }
    }
}
