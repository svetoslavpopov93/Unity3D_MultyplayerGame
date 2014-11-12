using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Continental.Shared;

namespace Continental.Games
{
	public class Player : IGameCommand
	{
		public string username;
		public string password;
		public ulong playerId;
		public bool enabled;

		public IGameCommand belongings;
		public void Serialize(System.IO.BinaryWriter bw, short command)
		{
			switch (command)
			{
				// upon registration you should receive back only userID
				case NetworkCommands.REGISTER_RESPONSE_OK:
				case NetworkCommands.AUTHORIZE_RESPONSE_OK:
                    bw.Write(username);
                    bw.Write(password);
					bw.Write(playerId);
					break;
				case NetworkCommands.REGISTER_REQUEST:
				case NetworkCommands.AUTHORIZE_REQUEST:
					SerializationManager.SafeWriteString(username, bw);
					SerializationManager.SafeWriteString(password, bw);
					break;
				
			}
		}

		public void Deserialize(System.IO.BinaryReader br, short command)
		{
			switch (command)
			{
				case NetworkCommands.REGISTER_RESPONSE_OK:
                case NetworkCommands.AUTHORIZE_RESPONSE_OK:
                    username = br.ReadString();
                    password = br.ReadString();
					playerId = br.ReadUInt64();
					break;
				case NetworkCommands.REGISTER_REQUEST:
				case NetworkCommands.AUTHORIZE_REQUEST:
					username = SerializationManager.SafeReadString(br);
					password = SerializationManager.SafeReadString(br);
					break;
			}
		}

		public new string ToString()
		{
			return string.Format("Player ID:{0} Username:{1}", playerId, username);
		}
	}
}
