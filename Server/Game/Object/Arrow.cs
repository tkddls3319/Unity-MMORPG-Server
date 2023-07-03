using Google.Protobuf.Protocol;
using Server.Game.Object;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Game
{
    public class Arrow : Projectile
    {
        public GameObject Owner { get; set; }

        long _nextMoveTick = 0;
        public override void Update()
        {
            if (Data == null || Data.projectile  == null || Owner == null || Room == null) return;

            if (_nextMoveTick >= Environment.TickCount64)
                return;

            long tick = (long)(1000 / Data.projectile.speed) ;
            _nextMoveTick = Environment.TickCount64 + tick;

            vector2Int destPos = GetFrontCellPos();
            if (Room.Map.CanGo(destPos))
            {
                CellPos = destPos;

                S_Move movePacket = new S_Move();
                movePacket.ObjectId = Id;
                movePacket.PosInfo = PosInfo;

                Room.BroadCast(movePacket);
            }
            else
            {
                //todo 피격
                GameObject target = Room.Map.Find(destPos);
                if(target != null)
                {
                    target.OnDamaged(this, Data.damage + Owner.Stat.Attack);
                }
                //소멸
                Room.Push(Room.LeaveGame, Id);
            }
        }
    }
}
