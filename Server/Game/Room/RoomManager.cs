using Google.Protobuf.Protocol;
using Google.Protobuf.WellKnownTypes;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Numerics;
using System.Text;

namespace Server.Game
{
    public class RoomManager : JobSerializer
    {
        public static RoomManager Instance { get; } = new RoomManager();

        object _lock = new object();

        Dictionary<int, GameRoom> _rooms = new Dictionary<int, GameRoom>();
        
        int _roomId = 1;

        public void Update()
        {
            Flush();

            foreach (GameRoom room in _rooms.Values)
            {
                room.Update();
            }

        }

        //public GameRoom AddLobby(C_RoomCreate packet = null, ClientSession player = null)
        //{
            //GameRoom room = new GameRoom();

            //lock (_lock)
            //{
            //    room.roomInfor.RoomId = _roomId;
            //    if (packet != null)
            //    {
            //        room.roomInfor.RoomMaster = player.MyPlayer.Info;
            //        room.roomInfor.Name = packet.Name;
            //        room.roomInfor.UserMax = packet.UserCount;
            //    }
            //    _rooms.Add(_roomId, room);
            //    _roomId++;
            //    return room;
            //}
       // }

        public GameRoom Add(int mapId, C_RoomCreate packet = null, ClientSession player = null)
        {
            GameRoom room = new GameRoom();
                room.Push(room.Init, mapId);
            lock (_lock)
            {
                room.roomInfor.RoomId = _roomId;
                if(packet != null)
                {
                    room.roomInfor.RoomMaster = player.MyPlayer.Info;
                    room.roomInfor.Name = packet.Name;
                    room.roomInfor.UserMax = packet.UserCount;
                    room.roomInfor.RoomId = _roomId;
                }
                _rooms.Add(_roomId, room);
                _roomId++;
                return room;
            }
        }

        public bool ReMove(int roomID)
        {
            lock (_lock)
            {
                return _rooms.Remove(roomID);
            }
        }

        /// <summary>
        /// 1번 room은 로비
        /// </summary>
        /// <param name="roomID"></param>
        /// <returns></returns>
        public GameRoom Find(int roomID)
        {
            lock (_lock)
            {
                GameRoom room = null;

                if (_rooms.TryGetValue(roomID, out room))
                    return room;
                return room;
            }
        }

        public List<GameRoom> AllRoom()
        {
            List<GameRoom> rooms = new List<GameRoom>();
            lock (_lock)
            {
                foreach (GameRoom room in _rooms.Values)
                {
                    rooms.Add(room);
                }
                return rooms;
            }
        }

        public int Count()
        {
            lock(_lock)
            {
                return _rooms.Count;
            }
        }
    }
}
