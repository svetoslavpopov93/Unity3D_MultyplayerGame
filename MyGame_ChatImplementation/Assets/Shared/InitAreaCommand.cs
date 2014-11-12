using System;
using System.Collections.Generic;
using System.Text;
using Continental.Shared;
namespace Continental.Games
{
	public class InitAreaCommand : IGameCommand
	{
		/// <summary>
		/// This message is sent from server to client when a player connects.
		/// </summary>
		public List<PlayerCharacter> PlayersInGame;

		public void Deserialize(System.IO.BinaryReader br, short command)
		{
			switch (command)
			{
				case NetworkCommands.INIT_AREA:
					uint playersCount = br.ReadUInt32();
					PlayersInGame = new List<PlayerCharacter>((int)playersCount);
					for (uint index = 0; index < playersCount; index++)
					{
						PlayerCharacter pc = new PlayerCharacter();
						pc.Deserialize(br, command);
						PlayersInGame.Add(pc);
					}
					break;
				default:
					throw new NotSupportedException(string.Format("Command option:{0} is not supported", command));
			}
		}

		public void Serialize(System.IO.BinaryWriter bw, short command)
		{
			switch (command)
			{
				case NetworkCommands.INIT_AREA:
					bw.Write((uint)PlayersInGame.Count);
					foreach (PlayerCharacter pc in PlayersInGame)
					{
						pc.Serialize(bw, command);
					}
					break;

				default:
					throw new NotSupportedException(string.Format("Command option:{0} is not supported", command));
			}
		}
	}
}
