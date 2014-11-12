using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Continental.Shared;
using System.Data;
using Npgsql;
using Continental.GameCore;

namespace Continental.Games
{
    /// <summary>
    /// Implementation of GameBase class.
    /// This source file is developed by a game developer. It remains on server side. It stays into the Y-Emulator project or it gets copied and compiled to the Game Cloud.
    /// This class implements the game logic. Game logic remains on server side only. Player machines do not have access to it. The code implements the authoritative game server mode.
    /// </summary>
    public class GameCore : GameBase
    {
        /// <summary>
        /// Defines player belongings to all players
        /// </summary>
        public Dictionary<ulong, PlayerCharacter> PlayersInGame;

        public Dictionary<ulong, Player> playersAll;

        private Random RandomGenerator;

        /// <summary>
        /// When player connects to the server his player becomes active
        /// </summary>
        /// <param name="playerId"></param>
        /// <param name="playerName"></param>
        public override ConnectionResponse OnConnect(short context, IGameCommand command, ulong connectionId)
        {

            ConnectionResponse response = new ConnectionResponse();
            //return new ConnectionResponse();
            try
            {
                switch (context)
                {
                    case NetworkCommands.AUTHORIZE_REQUEST:

                        foreach (Player plr in playersAll.Values)
                        {
                            if (plr.username == ((Player)command).username && plr.password == ((Player)command).password)
                            {
                                if (PlayersInGame.ContainsKey(plr.playerId))
                                {
                                    ulong oldConnId = PlayersInGame[plr.playerId].connectionId;
                                    OnConnectionClosed(oldConnId);
                                    response.connectionSuccess = false;
                                    response.responseCommand = new ClosedConnectionInfo { reason = ClosedConnectionInfo.DisconnectedReason.LoggedFromDifferentLocation };
                                    return response;
                                }

                                PlayersInGame.Add(plr.playerId, LoadAndPlaceCharacter(playersAll[plr.playerId], connectionId));
                                response.connectionSuccess = true;
                                response.responseCommand = plr;
                                response.responseContext = NetworkCommands.AUTHORIZE_RESPONSE_OK;
                                NotifyPlayerAdded(PlayersInGame[plr.playerId]);
                                return response;
                            }

                        }
                        response.connectionSuccess = false;
                        response.responseContext = NetworkCommands.AUTHORIZE_RESPONSE_FAIL;
                        return response;
                    case NetworkCommands.REGISTER_REQUEST:
                        Player player = (Player)command;
                        PlayerRegisterResponse respMsg = new PlayerRegisterResponse();

                        player.username = player.username.Trim();
                        Player plrExisting = playersAll.Values.Where(w => w.username.Equals(player.username, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                        if (plrExisting != null)
                        {
                            //send fail message
                            respMsg.result = PlayerRegisterResult.UsernameBusy;
                            response.responseCommand = respMsg;
                            response.responseContext = NetworkCommands.REGISTER_RESPONSE_FAIL;
                            response.connectionSuccess = false;
                            return response;
                        }
                        else
                        {
                            string sql = "insert into players (password, username, is_active) values (:password, :username, true) returning id";
                            NpgsqlParameter[] parameters = new NpgsqlParameter[2];
                            parameters[0] = new NpgsqlParameter("password", DbType.String);
                            parameters[0].Value = player.password;
                            parameters[1] = new NpgsqlParameter("username", DbType.String);
                            parameters[1].Value = player.username;
                            int id = (int)ExecuteScalar(sql, parameters);
                            player.playerId = (ulong)id;
                            lock (playersAll)
                            {
                                playersAll.Add(player.playerId, player);
                            }
                            respMsg.result = PlayerRegisterResult.OK;
                        }

                        if (respMsg.result == PlayerRegisterResult.OK)
                        {
                            PlayersInGame.Add(player.playerId, LoadAndPlaceCharacter(playersAll[player.playerId], connectionId));
                            response.connectionSuccess = true;
                            response.responseCommand = player;
                            response.responseContext = NetworkCommands.AUTHORIZE_RESPONSE_OK;
                            NotifyPlayerAdded(PlayersInGame[player.playerId]);
                            return response;
                        }
                        break;
                    case NetworkCommands.AUTHORIZE_GUEST:
                        Player guestPlayer = new Player() { username = "guest", password = "guestPassword", enabled = false, playerId = 1 };
                        respMsg = new PlayerRegisterResponse();

                        guestPlayer.username = guestPlayer.username.Trim();
                        Player plrExistingg = playersAll.Values.Where(w => w.playerId.Equals(guestPlayer.playerId)).FirstOrDefault();
                        
                        bool playerExists = false;

                        foreach (var plr in PlayersInGame)
                        {
                            if (plr.Value.Name == guestPlayer.username)
                            {
                                playerExists = true;
                                break;
                            }
                        }

                        if (playerExists)
                        {
                            for (int i = 1; i <= 501; i++)
                            {
                                guestPlayer.username += "" + i;

                                plrExistingg = playersAll.Values.Where(w => w.username.Equals(guestPlayer.username, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();

                                if (plrExistingg == null)
                                {
                                    break;
                                }
                                else if (true)
                                {
                                    throw new Exception("Maximum symbol count for guest name reached.");
                                }
                            }
                        }

                        guestPlayer.playerId = GenerateGuestId();

                        PlayersInGame.Add(guestPlayer.playerId, LoadAndPlaceCharacter(guestPlayer, connectionId));
                        response.connectionSuccess = true;
                        response.responseCommand = guestPlayer;
                        response.responseContext = NetworkCommands.AUTHORIZE_RESPONSE_OK;
                        NotifyPlayerAdded(PlayersInGame[guestPlayer.playerId]);
                        return response;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            response.connectionSuccess = false;
            return response;
        }

        /// <summary>
        /// Generates new playerId for the current guest player.
        /// </summary>
        /// <returns></returns>
        private ulong GenerateGuestId()
        {
            for (ulong i = 1500; i <= 2000; i++)
            {
                if (!PlayersInGame.ContainsKey((ulong)i))
                {
                    return i;
                }
            }

            throw new Exception("No more free slots for guests.");
        }

        private bool CheckForId(Player pl)
        {
            bool containsId = false;

            foreach (var plr in PlayersInGame)
            {
                if (plr.Value.playerId == pl.playerId)
                {
                    return containsId = true;
                }
            }

            return false;
        }

        /// <summary>
        /// Called when player goes offline.
        /// </summary>
        /// <param name="playerId"></param>
        public override void OnConnectionClosed(ulong connectionId)
        {
            foreach (PlayerCharacter plr in PlayersInGame.Values.ToList())
            {
                if (plr.connectionId == connectionId)
                {
                    plr.IsOnline = false;
                    //tell other players someone left
                    NotifyOnlineStateChanged(plr);
                    PlayersInGame.Remove(plr.playerId);
                }
            }
        }

        /// <summary>
        /// Called once the game server is started
        /// </summary>
        public override void OnStart()
        {
            playersAll = new Dictionary<ulong, Player>();
            string sql = "select * from players where is_active=true";
            DataSet players = GetDataSet(sql);
            foreach (DataTable table in players.Tables)
            {
                foreach (DataRow row in table.Rows)
                {
                    playersAll.Add(ulong.Parse(row["id"].ToString()), new Player { playerId = ulong.Parse(row["id"].ToString()), username = row["username"].ToString(), password = row["password"].ToString() });
                }
            }
            // there should be no more than 1500 registered players in this game
            if (playersAll.Count > 1500)
            {
                throw new Exception("Players limit exceeded!");
            }
            PlayersInGame = new Dictionary<ulong, PlayerCharacter>();
            RandomGenerator = new Random();
        }

        /// <summary>
        /// Method is called when the administrative interface requires the game to stop running.
        /// The command can come from game developer request or automatically after Game Cloud Continental rules are broken.
        /// </summary>
        public override void OnStop()
        {
            //Nothing to do here for this GameBase implementation. For your implementation you will need to handle this event correctly.
            //For example save any unsaved states to the Database 
        }



        /// <summary>
        /// Each game has a timing generator. It is called every 1/60 seconds. All actions done inside must complete withing 1/60 seconds. If this rule is broken a warning message is logged.
        /// If this rule is flagrantly violated the game can be requested to shut-down.
        /// Use asynchronous calls for working with database to avoid unexpected delays in the execution
        /// </summary>
        public override void OnFrame(float deltatime)
        {
            foreach (ulong playerId in PlayersInGame.Keys)
            {
                PlayerCharacter ypc = PlayersInGame[playerId];
                //update each online player move
                ypc.Update(deltatime);
            }
        }

        /// <summary>
        /// Called when a command from player arrives. Login and register requests are already handled by the GCC system.
        /// </summary>
        /// <param name="context">Context of the message. See more at Context Serialization in our online documentation.</param>
        /// <param name="command">Message class implementing IGameCommand</param>
        /// <param name="playerId">Player who sends the command. GCC has filtered unauthorized or malicious requests before calling this method.</param>
        public override void OnCommand(ulong connectionId, short context, IGameCommand command) //ulong connectionId, short context, IGameCommand command
        {
            switch (context)
            {
                case NetworkCommands.PLAYER_CHARACTER_STATE_CHANGE:
                    PlayersInGame[((PlayerCharacter)command).playerId].UpdateState((PlayerCharacter)command, false);
                    NotifyStateChanged(PlayersInGame[((PlayerCharacter)command).playerId]);
                    break;
                case NetworkCommands.GET_AREA:

                    SendInitArea(connectionId);
                    foreach (PlayerCharacter plr in PlayersInGame.Values.ToList())
                    {
                        if (connectionId == plr.connectionId)
                        {
                            plr.isInGame = true;
                        }
                    }
                    break;
                case NetworkCommands.MESSAGE_SEND:
                    string sender = GetMessageSender(connectionId);
                    MessageFromServer curr = new MessageFromServer() { Sender = sender, Msg = ((Message)command).Msg};
                    command = curr;
                    //Message curr = (Message)command;
                    Console.WriteLine(curr.Msg);
                    foreach (var pl in PlayersInGame)
                    {
                        List<ulong> players = new List<ulong>();

                        foreach (var plr in PlayersInGame)
                        {
                            players.Add(plr.Value.connectionId);
                        }

                        SendToMultipleConnections(NetworkCommands.MESSAGE_RECIEVE, command, players);

                        Console.WriteLine("Message send back to clients!");
                    }

                    break;
                default:
                    throw new InvalidOperationException(string.Format("Received not supported command:{0} context:{1}, connectionId:{2}",
                        command, context, connectionId));
            }
        }

        private string GetMessageSender(ulong id)
        {
            string senderName = "";

            foreach (var plr in PlayersInGame)
            {
                if (id == plr.Value.connectionId)
                {
                    senderName = plr.Value.Name;
                    return senderName;
                }
            }

            throw new ArgumentException("User with the current id is not found.");
        }

        /// <summary>
        /// Send message to all online players that somebody's got online or offline
        /// </summary>
        /// <param name="updatedPlayer"></param>
        private void NotifyOnlineStateChanged(PlayerCharacter updatedPlayer)
        {
            List<ulong> recipients = new List<ulong>();
            foreach (PlayerCharacter plr in PlayersInGame.Values)
            {
                if (plr.playerId != updatedPlayer.playerId)
                {
                    recipients.Add(plr.connectionId);
                }
            }
            if (recipients.Count > 0)
            {
                SendToMultipleConnections(NetworkCommands.ONLINE_STATE_CHANGE, updatedPlayer, recipients);
            }
        }

        /// <summary>
        /// Send message to all online players that somebody has moved or turned
        /// </summary>
        /// <param name="updatedPlayer"></param>
        private void NotifyStateChanged(PlayerCharacter updatedPlayer)
        {
            List<ulong> recipients = new List<ulong>();
            foreach (PlayerCharacter plr in PlayersInGame.Values)
            {
                if (plr.isInGame)
                {
                    recipients.Add(plr.connectionId);
                }
            }
            if (recipients.Count > 0)
            {
                SendToMultipleConnections(NetworkCommands.PLAYER_CHARACTER_STATE_CHANGE, updatedPlayer, recipients);
            }
        }

        /// <summary>
        /// Send message to all online players that a new player was registered
        /// </summary>
        /// <param name="addedPlayer"></param>
        private void NotifyPlayerAdded(PlayerCharacter addedPlayer)
        {
            List<ulong> recipients = new List<ulong>();
            foreach (PlayerCharacter plr in PlayersInGame.Values)
            {
                if (plr.playerId != addedPlayer.playerId)
                {
                    recipients.Add(plr.connectionId);
                }
            }
            if (recipients.Count > 0)
            {
                SendToMultipleConnections(NetworkCommands.PLAYER_ADDED, addedPlayer, recipients);
            }
        }

        /// <summary>
        /// Send initial data to a just connected player
        /// </summary>
        /// <param name="playerID"></param>
        private void SendInitArea(ulong connectionId)
        {
            InitAreaCommand initArea = new InitAreaCommand();
            initArea.PlayersInGame = PlayersInGame.Values.ToList();
            SendToSingleConnection(connectionId, NetworkCommands.INIT_AREA, initArea);

        }

        /// <summary>
        /// Init player with random starting values for position, rotation and color
        /// </summary>
        /// <param name="pl"></param>
        /// <returns></returns>
        private PlayerCharacter LoadAndPlaceCharacter(Player pl, ulong connectionId)
        {
            return new PlayerCharacter
            {
                playerId = pl.playerId,
                IsOnline = true,
                isInGame = false,
                Movement = PlayerCharacter.MovementState.Motionless,
                Name = pl.username,
                connectionId = connectionId,
                Position = new Continental.Games.Vector2
                {
                    X = RandomGenerator.Next(Globals.MIN_X + 2, Globals.MAX_X - 2),
                    Y = RandomGenerator.Next(Globals.MIN_Y + 2, Globals.MAX_Y - 2)
                },
                Rotation = PlayerCharacter.RotationState.Motionless,
                RotationAngle = RandomGenerator.Next(0, 359),
                Color = new Continental.Games.Color
                {
                    A = 255,
                    R = (byte)RandomGenerator.Next(byte.MinValue, byte.MaxValue),
                    G = (byte)RandomGenerator.Next(byte.MinValue, byte.MaxValue),
                    B = (byte)RandomGenerator.Next(byte.MinValue, byte.MaxValue)
                }
            };
        }
    }

}
