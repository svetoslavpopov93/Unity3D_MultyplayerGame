using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Continental.Shared;

namespace Continental.Games
{
	public enum PlayerRegisterResult
	{
		OK,
		UsernameBusy,
		WeakPassword,
		Banned,
		ServerError,
		LoggedInAlready
	}
	public class PlayerRegisterResponse : IGameCommand
	{
		public PlayerRegisterResult result;
		public void Serialize(System.IO.BinaryWriter bw, short command)
		{
			bw.Write((short)result);
		}

		public void Deserialize(System.IO.BinaryReader br, short command)
		{
			result = (PlayerRegisterResult)br.ReadInt16();
		}
		public override string ToString()
		{
			return result.ToString();
		}
	}
}
