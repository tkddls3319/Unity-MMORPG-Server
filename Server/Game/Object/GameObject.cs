using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Game.Object
{
    public class GameObject
    {
        public GameObjectType ObjectType { get; set; } = GameObjectType.None;
        public int Id
        {
            get { return Info.ObjectId; }
            set
            {
                Info.ObjectId = value;
            }
        }
        public GameRoom Room { get; set; }
        public ObjectInfo Info { get; set; } = new ObjectInfo() ;
        public PositionInfo PosInfo { get; private set; } = new PositionInfo();
        public StatInfo Stat { get; private set; } = new StatInfo();

        public float Speed
        {
            get{ return Stat.Speed; }
            set{ Stat.Speed = value; }
        }

        public GameObject()
        {
            Info.PosInfo = PosInfo;
            Info.StatInfo = Stat;
        }

        public vector2Int CellPos
        {
            get
            {
                return new vector2Int(PosInfo.PosX, PosInfo.PosY);
            }
            set
            {
                PosInfo.PosX = value.x;
                PosInfo.PosY = value.y;
            }
        }
        public vector2Int GetFrontCellPos()
        {
            return GetFrontCellPos(PosInfo.MoveDir);
        }
        /// <summary>
        /// 내가 바라보고있는 셀의 위치
        /// </summary>
        /// <returns></returns>
        public vector2Int GetFrontCellPos(MoveDir dir)
        {
            vector2Int cellPos = CellPos;

            switch (dir)
            {
                case MoveDir.Up:
                    cellPos += vector2Int.up;
                    break;
                case MoveDir.Down:
                    cellPos += vector2Int.down;
                    break;
                case MoveDir.Left:
                    cellPos += vector2Int.left;
                    break;
                case MoveDir.Right:
                    cellPos += vector2Int.right;
                    break;
            }
            return cellPos;
        }
        public virtual void OnDamaged(GameObject attacker, int damage) 
        {
            if (Room == null)
                return;

            Stat.Hp = Math.Max(Stat.Hp - damage, 0);

            S_ChangeHp changePacket = new S_ChangeHp();
            changePacket.ObjectId = Id;
            changePacket.Hp = Stat.Hp;
            Room.BroadCast(changePacket);

            if (Stat.Hp <= 0)
            {
                OnDead(attacker);
            }
        
        }
        public virtual void OnDead(GameObject attacker)
        {
            if(Room == null)
                return;
        }
    }
}
