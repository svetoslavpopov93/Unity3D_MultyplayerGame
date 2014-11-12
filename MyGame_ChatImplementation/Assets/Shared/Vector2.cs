using System;
using Continental.Shared;
namespace Continental.Games
{
	/// <summary>
	/// A serializable version of Unity's Vector2D
	/// </summary>
	public struct Vector2 : IGameCommand
	{
		public float X;
		public float Y;

		public void Deserialize(System.IO.BinaryReader br, short command)
		{
			X = br.ReadSingle();
			Y = br.ReadSingle();
		}

		public void Serialize(System.IO.BinaryWriter bw, short command)
		{
			bw.Write(X);
			bw.Write(Y);
		}

		public override string ToString()
		{
			return string.Format("X:{0:#.###}, Y:{1:#.###}", X, Y);
		}
	}
}
