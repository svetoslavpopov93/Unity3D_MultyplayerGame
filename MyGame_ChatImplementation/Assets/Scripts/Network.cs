using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Continental;
using Continental.Games;
using System;
using Continental.Shared;


/// <summary>
/// This script handles incomming network commands from server.
/// </summary>
public class Network : MonoBehaviour
{
    /// <summary>
    /// A collection of all online players
    /// </summary>
    Dictionary<ulong, PlayerCharacter> playersInGame;
    private bool areaInitialized = false;

    void Start()
    {
        Texture2D grass = Resources.Load("Grass") as Texture2D;
        GameObject.Find("Plane").renderer.material.mainTextureScale = new UnityEngine.Vector2(10, 10);
        GameObject.Find("Plane").renderer.material.mainTexture = grass;
        playersInGame = new Dictionary<ulong, PlayerCharacter>();

        // request area information from server
        Login.connection.SendAndReceive();
        Login.connection.Send(NetworkCommands.GET_AREA, null);
        Login.connection.SendAndReceive();
    }

    /// <summary>
    /// Add player game object to unity scene.
    /// </summary>
    /// <param name="player"></param>
    private void AddPlayerCharacterToGame(PlayerCharacter player)
    {
        UnityEngine.Object prefab = Resources.Load("PlayerCharacter");
        GameObject go = (GameObject)GameObject.Instantiate(prefab);
        go.GetComponent<PlayerCharacterScript>().PlayerCharacter = player;
        go.transform.position = new Vector3(player.Position.X, 1, player.Position.Y);
    }


    /// <summary>
    /// On application exit or scene change we stop the network communication.
    /// </summary>
    void OnDestroy()
    {
        Login.connection.Disconnect(false);// .Stop();
    }

    /// <summary>
    /// We need to call CommandQueue.GetIncomingCommands() on each update. It can read between zero and many enqueued messages from the game server.
    /// </summary>
    void Update()
    {
        Login.connection.SendAndReceive();
        IEnumerable<CommandPair> commands = Login.connection.GetIncomingCommands();
        foreach (CommandPair pair in commands)
        {
            if (!areaInitialized && pair.Context != NetworkCommands.INIT_AREA)
                continue;
            switch (pair.Context)
            {
                case NetworkCommands.INIT_AREA:
                    foreach (PlayerCharacter player in ((InitAreaCommand)(pair.Data)).PlayersInGame)
                    {
                        AddPlayerCharacterToGame(player);
                        playersInGame.Add(player.playerId, player);
                    }
                    areaInitialized = true;
                    break;
                case NetworkCommands.ONLINE_STATE_CHANGE:
                    playersInGame[((PlayerCharacter)pair.Data).playerId].IsOnline = ((PlayerCharacter)pair.Data).IsOnline;
                    if (!playersInGame[((PlayerCharacter)pair.Data).playerId].IsOnline)
                    {
                        playersInGame.Remove(((PlayerCharacter)pair.Data).playerId);
                    }
                    break;
                case NetworkCommands.PLAYER_CHARACTER_STATE_CHANGE:
                    if (playersInGame.ContainsKey(((PlayerCharacter)pair.Data).playerId))
                    {
                        playersInGame[((PlayerCharacter)pair.Data).playerId].UpdateState(
                            (PlayerCharacter)pair.Data, true, ((PlayerCharacter)pair.Data).playerId != Login.myPlayer.playerId);
                    }
                    break;
                case NetworkCommands.PLAYER_ADDED:
                    if (!playersInGame.ContainsKey(((PlayerCharacter)pair.Data).playerId))
                    {
                        AddPlayerCharacterToGame((PlayerCharacter)pair.Data);
                        playersInGame.Add(((PlayerCharacter)pair.Data).playerId, (PlayerCharacter)pair.Data);
                    }
                    break;
                case CommandCodes.CONNECTION_CLOSED:
                    Login.connection.Disconnect(false);
                    break;
                case NetworkCommands.MESSAGE_RECIEVE:
                    string current = ((MessageFromServer)pair.Data).ToString();   
                //string current = ((Message)pair.Data).ToString();
                    if (current != ChatScript.lastMessage)
                    {
                        ChatScript.messBox += current;
                        ChatScript.lastMessage = current;
                    }
                    break;
                default:
                    Debug.LogError(string.Format("Unsupported command {0} received!", pair.Context));
                    break;
            }
        }
    }
}
