using DummyClient;
using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Text;

public class PacketHandler
{
    public static void S_ChatHandler(PacketSession session, IMessage packet)
    {
        ServerSession clientSession = (ServerSession)session;
        S_Chat s_chat = (S_Chat)packet;


        Console.WriteLine(s_chat.Chat);
    }
}
