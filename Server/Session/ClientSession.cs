using Google.Protobuf;
using Google.Protobuf.Protocol;
using Microsoft.Win32.SafeHandles;
using Server;
using Server.Data;
using Server.Game;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;

namespace ServerCore
{
    public partial class ClientSession : PacketSession
    {
        public Player MyPlayer { get; set; }
        public int SessionID { get; set; }

        public void Send(IMessage packet)
        {
            string packetName = packet.Descriptor.Name.Replace("_", string.Empty);
            PacketID packetID = (PacketID)Enum.Parse(typeof(PacketID), packetName);

            ushort size = (ushort)packet.CalculateSize();
            byte[] sendBuffer = new byte[size + 4];

            Array.Copy(BitConverter.GetBytes((ushort)(size + 4)), 0, sendBuffer, 0, sizeof(ushort));
            Array.Copy(BitConverter.GetBytes((ushort)(packetID)), 0, sendBuffer, sizeof(ushort), sizeof(ushort));
            Array.Copy(packet.ToByteArray(), 0, sendBuffer, 4, size);


            Console.WriteLine($"SEND - Packet : {packet.Descriptor.Name}, SessionId : {SessionID}");
            Send(new ArraySegment<byte>(sendBuffer));
        }

        public override void OnConnected(EndPoint endPoint)
        {
          
            Console.WriteLine($"OnConnected - IP : {endPoint}, SessionId : {SessionID}");
            //GameRoom room = RoomManager.Instance.Find(1);
            //room.Push(room.EnterGame, MyPlayer);

        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            GameRoom room = RoomManager.Instance.Find(1);
            room.Push(room.LeaveGame, MyPlayer.Info.ObjectId);
            SessionManager.Instance.Remove(this);
            Console.WriteLine($"OnDisconnected - IP : {endPoint}");
        }

        public override void OnRecvedPacket(ArraySegment<byte> buffer)
        {
            PacketManager.Instance.OnRecvPacket(this, buffer);
        }

        public override void OnSended(int numOfByte)
        {

        }
    }
}
