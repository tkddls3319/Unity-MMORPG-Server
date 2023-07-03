using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;
using System;
using System.Collections.Generic;

class PacketManager
{
	#region Singleton
	static PacketManager _instance = new PacketManager();
	public static PacketManager Instance { get { return _instance; } }
	#endregion

	PacketManager()
	{
		Register();
	}

	Dictionary<ushort, Action<PacketSession, ArraySegment<byte>, ushort>> _onRecv = new Dictionary<ushort, Action<PacketSession, ArraySegment<byte>, ushort>>();
	Dictionary<ushort, Action<PacketSession, IMessage>> _handler = new Dictionary<ushort, Action<PacketSession, IMessage>>();
		
	public Action<PacketSession, IMessage, ushort> CustomHandler { get; set; }

	public void Register()
	{		
		_onRecv.Add((ushort)PacketID.CLogin, MakePacket<C_Login>);
		_handler.Add((ushort)PacketID.CLogin, PacketHandler.C_LoginHandler);		
		_onRecv.Add((ushort)PacketID.CChat, MakePacket<C_Chat>);
		_handler.Add((ushort)PacketID.CChat, PacketHandler.C_ChatHandler);		
		_onRecv.Add((ushort)PacketID.CMove, MakePacket<C_Move>);
		_handler.Add((ushort)PacketID.CMove, PacketHandler.C_MoveHandler);		
		_onRecv.Add((ushort)PacketID.CSkill, MakePacket<C_Skill>);
		_handler.Add((ushort)PacketID.CSkill, PacketHandler.C_SkillHandler);		
		_onRecv.Add((ushort)PacketID.CEnterGame, MakePacket<C_EnterGame>);
		_handler.Add((ushort)PacketID.CEnterGame, PacketHandler.C_EnterGameHandler);		
		_onRecv.Add((ushort)PacketID.CRoomCreate, MakePacket<C_RoomCreate>);
		_handler.Add((ushort)PacketID.CRoomCreate, PacketHandler.C_RoomCreateHandler);
	}

	public void OnRecvPacket(PacketSession session, ArraySegment<byte> buffer)
	{
		ushort count = 0;

		ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
		count += 2;
		ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
		count += 2;

		Action<PacketSession, ArraySegment<byte>, ushort> action = null;
		if (_onRecv.TryGetValue(id, out action))
			action.Invoke(session, buffer, id);
	}

	void MakePacket<T>(PacketSession session, ArraySegment<byte> buffer, ushort id) where T : IMessage, new()
	{
		T pkt = new T();
		pkt.MergeFrom(buffer.Array, buffer.Offset + 4, buffer.Count - 4);

		if (CustomHandler != null)
		{
			CustomHandler.Invoke(session, pkt, id);
		}
		else
		{
			Action<PacketSession, IMessage> action = null;
			if (_handler.TryGetValue(id, out action))
				action.Invoke(session, pkt);
		}
	}

	public Action<PacketSession, IMessage> GetPacketHandler(ushort id)
	{
		Action<PacketSession, IMessage> action = null;
		if (_handler.TryGetValue(id, out action))
			return action;
		return null;
	}
}