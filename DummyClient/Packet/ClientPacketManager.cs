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
		_onRecv.Add((ushort)PacketID.SLogin, MakePacket<S_Login>);
		_handler.Add((ushort)PacketID.SLogin, PacketHandler.S_LoginHandler);		
		_onRecv.Add((ushort)PacketID.SChat, MakePacket<S_Chat>);
		_handler.Add((ushort)PacketID.SChat, PacketHandler.S_ChatHandler);		
		_onRecv.Add((ushort)PacketID.SEnterGame, MakePacket<S_EnterGame>);
		_handler.Add((ushort)PacketID.SEnterGame, PacketHandler.S_EnterGameHandler);		
		_onRecv.Add((ushort)PacketID.SLeaveGame, MakePacket<S_LeaveGame>);
		_handler.Add((ushort)PacketID.SLeaveGame, PacketHandler.S_LeaveGameHandler);		
		_onRecv.Add((ushort)PacketID.SSpawn, MakePacket<S_Spawn>);
		_handler.Add((ushort)PacketID.SSpawn, PacketHandler.S_SpawnHandler);		
		_onRecv.Add((ushort)PacketID.SDespawn, MakePacket<S_Despawn>);
		_handler.Add((ushort)PacketID.SDespawn, PacketHandler.S_DespawnHandler);		
		_onRecv.Add((ushort)PacketID.SMove, MakePacket<S_Move>);
		_handler.Add((ushort)PacketID.SMove, PacketHandler.S_MoveHandler);		
		_onRecv.Add((ushort)PacketID.SSkill, MakePacket<S_Skill>);
		_handler.Add((ushort)PacketID.SSkill, PacketHandler.S_SkillHandler);		
		_onRecv.Add((ushort)PacketID.SChangeHp, MakePacket<S_ChangeHp>);
		_handler.Add((ushort)PacketID.SChangeHp, PacketHandler.S_ChangeHpHandler);		
		_onRecv.Add((ushort)PacketID.SGameRoom, MakePacket<S_GameRoom>);
		_handler.Add((ushort)PacketID.SGameRoom, PacketHandler.S_GameRoomHandler);		
		_onRecv.Add((ushort)PacketID.SRoomCreate, MakePacket<S_RoomCreate>);
		_handler.Add((ushort)PacketID.SRoomCreate, PacketHandler.S_RoomCreateHandler);
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