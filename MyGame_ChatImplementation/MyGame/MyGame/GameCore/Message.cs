using Continental.Shared;
using System;

namespace Continental.GameCore
{
    public class Message : IGameCommand
    {
        public string Msg;

        public void Deserialize(System.IO.BinaryReader br, short command)
        {
            Msg = br.ReadString();
        }
        public void Serialize(System.IO.BinaryWriter bw, short command)
        {
            bw.Write(Msg);
        }

        public override string ToString()
        {
            return this.Msg + "\n";
        }
    }
}