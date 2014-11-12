using System;
using Continental.Shared;
namespace Continental.Games
{
	public struct Color : IGameCommand
	{
		public byte A;
		public byte R;
		public byte G;
		public byte B;

		public void Deserialize(System.IO.BinaryReader br, short command)
		{
			A = br.ReadByte();
			R = br.ReadByte();
			G = br.ReadByte();
			B = br.ReadByte();
		}

		public void Serialize(System.IO.BinaryWriter bw, short command)
		{
			bw.Write(A);
			bw.Write(R);
			bw.Write(G);
			bw.Write(B);
		}
	}
}
