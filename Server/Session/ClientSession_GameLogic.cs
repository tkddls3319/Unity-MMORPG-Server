using Google.Protobuf.Protocol;
using Server.Data;
using Server.Game;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace ServerCore
{
    public partial class ClientSession : PacketSession
    {
        public void HandleLogin(C_Login loginPacket)
        {
            MyPlayer = ObjectManager.Instance.Add<Player>();
            {
                MyPlayer.Info.Name = $"Player_{MyPlayer.Info.ObjectId}";
                MyPlayer.Info.PosInfo.State = CreatureState.Idle;
                MyPlayer.Info.PosInfo.MoveDir = MoveDir.Down;
                MyPlayer.Info.PosInfo.PosX = 0;
                MyPlayer.Info.PosInfo.PosY = 0;

                #region json에서 받아온 데이터 파싱
                StatInfo stat = null;
                DataManager.StatDict.TryGetValue(1, out stat);
                MyPlayer.Stat.MergeFrom(stat);
                #endregion

                MyPlayer.Session = this;

                RoomManager.Instance.Push(() =>
                {
                    GameRoom lobbyRoom = RoomManager.Instance.Find(1);
                    lobbyRoom.Push(lobbyRoom.LoginGame, MyPlayer);

                });
            }
        }

        public void HandleEnterGame(C_EnterGame enterGamePacket)
        {
            RoomManager.Instance.Push(() =>
            {
                GameRoom gameRoom = RoomManager.Instance.Find(enterGamePacket.RoomId);
                gameRoom.Push(gameRoom.EnterGame, MyPlayer);
            });
        }

        public void HandleCreateRoom(C_RoomCreate enterGamePacket, ClientSession player)
        {
            RoomManager.Instance.Push(() =>
            {
                GameRoom lobbyRoom = RoomManager.Instance.Find(1);
                lobbyRoom.Push(lobbyRoom.LeaveGame, MyPlayer.Info.ObjectId);


                GameRoom room = RoomManager.Instance.Add(1, enterGamePacket, player);
                room.Push(room.RoomCreate, MyPlayer);

                RoomInfo newroomInfo = room.roomInfor;
                lobbyRoom.Push(lobbyRoom.RoomCreateBroadCast, newroomInfo);
            });
        }
        

    }
}
