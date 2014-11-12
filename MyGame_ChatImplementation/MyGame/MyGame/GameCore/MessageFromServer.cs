using Continental.Shared;
using System;

namespace Continental.GameCore
{
    public class MessageFromServer : IGameCommand
    {
        public string Msg;
        public string Sender;

        public void Deserialize(System.IO.BinaryReader br, short command)
        {
            Sender = br.ReadString();
            Msg = br.ReadString();
        }
        public void Serialize(System.IO.BinaryWriter bw, short command)
        {
            bw.Write(Sender);
            bw.Write(Msg);
        }

        public override string ToString()
        {
            return this.Sender + ": " + this.Msg + "\n";
        }
    }
}