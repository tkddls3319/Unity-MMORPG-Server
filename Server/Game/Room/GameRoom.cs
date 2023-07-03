using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server.Data;
using Server.Game.Object;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Numerics;
using System.Text;

namespace Server.Game
{
    public class GameRoom : JobSerializer
    {
        //TODO : 룸생성시

        public RoomInfo roomInfor { get; set; } = new RoomInfo();

        public int UserMax { get; set; }

        Dictionary<int, Player> _players = new Dictionary<int, Player>();
        Dictionary<int, Monster> _monsters = new Dictionary<int, Monster>();
        Dictionary<int, Projectile> _projectiles = new Dictionary<int, Projectile>();

        public Map Map { get; private set; } = new Map();
        public void Init(int mapId)
        {
            if (mapId != 0)
                Map.LoadMap(mapId);
        }
        /// <summary>
        /// 몬스터 및 투사체 이동 업데이트를 자동적으로 하기위해.
        /// </summary>
        public void Update()
        {
            foreach (Projectile projectile in _projectiles.Values)
            {
                Push(projectile.Update);
            }

            Flush();
        }
        public void LoginGame(GameObject gameObject)
        {
            if (gameObject == null)
                return;

            GameObjectType type = ObjectManager.GetObjectTypeById(gameObject.Id);
            if (type == GameObjectType.Player)
            {
                Player player = (Player)gameObject;
                _players.Add(player.Info.ObjectId, player);
                player.Room = this;

                //나에게 유저들 정보를 전달한다.
                {
                    S_Login loginOk = new S_Login();
                    loginOk.Player = player.Info;
                    player.Session.Send(loginOk);

                    S_Spawn spawnPacket = new S_Spawn();
                    foreach (Player p in _players.Values)
                    {
                        if (player != p)
                            spawnPacket.Objects.Add(p.Info);
                    }
                    player.Session.Send(spawnPacket);

                    S_GameRoom roomPacket = new S_GameRoom();
                    List<GameRoom> rooms = RoomManager.Instance.AllRoom();

                    foreach (GameRoom room in rooms)
                    {
                        if (room.roomInfor.RoomId != 1)
                            roomPacket.RoomInfo.Add(room.roomInfor);
                    }

                    player.Session.Send(roomPacket);
                }

                // 타인한테 정보 전송
                {
                    S_Spawn spawnPacket = new S_Spawn();
                    //foreach (Player  p in _players.Values)
                    //{
                    //spawnPacket.Objects.Add(p.Info);
                    //}
                    spawnPacket.Objects.Add(player.Info);

                    foreach (Player p in _players.Values)
                    {
                        if (p.Id != gameObject.Id)
                            p.Session.Send(spawnPacket);
                    }
                }
            }
        }

        public void RoomCreate(GameObject gameObject)
        {
            if (gameObject == null)
                return;

            S_RoomCreate s_RoomCreate = new S_RoomCreate();
            s_RoomCreate.RoomId = roomInfor.RoomId;

            Player player = (Player)gameObject;
            player.Session.Send(s_RoomCreate);

        }
        public void RoomCreateBroadCast(RoomInfo newroom)
        {
            S_GameRoom packet = new S_GameRoom();
            packet.RoomInfo.Add(newroom);
            foreach (Player player in _players.Values)
            {
                player.Session.Send(packet);
            }
        }

        public void EnterGame(GameObject gameObject)
        {
            if (gameObject == null)
                return;

            GameObjectType type = ObjectManager.GetObjectTypeById(gameObject.Id);
            if (type == GameObjectType.Player)
            {
                Player player = (Player)gameObject;
                _players.Add(player.Info.ObjectId, player);
                player.Room = this;

                Map.ApplyMove(player, new vector2Int(player.CellPos.x, player.CellPos.y));

                //본인에게 전달
                {
                    S_EnterGame enterPacket = new S_EnterGame();
                    player.Info.PosInfo.PosX = new Random().Next(1,4);
                    player.Info.PosInfo.PosY = new Random().Next(1,4);
                    enterPacket.Player = player.Info;
                    player.Session.Send(enterPacket);

                    S_Spawn spawnPacket = new S_Spawn();
                    foreach (Player p in _players.Values)
                    {
                        if (player != p)
                            spawnPacket.Objects.Add(p.Info);
                    }

                    foreach (Monster m in _monsters.Values)
                        spawnPacket.Objects.Add(m.Info);

                    foreach (Projectile p in _projectiles.Values)
                        spawnPacket.Objects.Add(p.Info);

                    player.Session.Send(spawnPacket);
                }
            }
            else if (type == GameObjectType.Monster)
            {
            }
            else if (type == GameObjectType.Projectile)
            {
                Projectile projectile = (Projectile)gameObject;
                _projectiles.Add(gameObject.Id, projectile);
                projectile.Room = this;
            }

            // 타인한테 정보 전송
            {
                S_Spawn spawnPacket = new S_Spawn();
                spawnPacket.Objects.Add(gameObject.Info);
                foreach (Player p in _players.Values)
                {
                    if (p.Id != gameObject.Id)
                        p.Session.Send(spawnPacket);
                }
            }
        }
        public void LeaveGame(int objectId)
        {
            GameObjectType type = ObjectManager.GetObjectTypeById(objectId);

            if (type == GameObjectType.Player)
            {
                Player player = null;
                if (_players.Remove(objectId, out player) == false)
                    return;

                Map.ApplyLeave(player);
                player.Room = null;//applyleave 에서 room = null 체크를 하기때문에 

                //본인에게 전달
                {
                    S_LeaveGame leavePacket = new S_LeaveGame();
                    player.Session.Send(leavePacket);
                }
            }
            else if (type == GameObjectType.Monster)
            {
            }
            else if (type == GameObjectType.Projectile)
            {
            }

            //타인에게 전달
            {
                S_Despawn despawnPacket = new S_Despawn();
                despawnPacket.ObjectIds.Add(objectId);

                foreach (Player p in _players.Values)
                {
                    if (p.Id != objectId)
                        p.Session.Send(despawnPacket);
                }
            }
        }
        public void HandleChat(Player player, C_Chat chatPacket)
        {
            if (player == null)
                return;

            string chat = chatPacket.Chat;
            ObjectInfo info = player.Info;

            if (string.IsNullOrEmpty(chat))
                return;

            S_Chat severChatPacket = new S_Chat()
            {
                PlayerId = info.ObjectId,
                Chat = chatPacket.Chat
            };
            BroadCast(severChatPacket);
        }
        /// <summary>
        /// 하나의 데이터를 변경하는 것은 위험하기 때문에 한곳에서만 이루어지게 한다.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="movePacket"></param>
        public void HandleMove(Player player, C_Move movePacket)
        {
            if (player == null)
                return;

            PositionInfo movePosInfo = movePacket.PosInfo;
            ObjectInfo info = player.Info;

            //다른좌표로 이동할 경우 갈수 있는지 체크
            if (movePosInfo.PosX != info.PosInfo.PosX || movePosInfo.PosY != info.PosInfo.PosY)
            {
                if (Map.CanGo(new vector2Int(movePosInfo.PosX, movePosInfo.PosY)) == false)
                    return;
            }
            info.PosInfo.State = movePosInfo.State;
            info.PosInfo.MoveDir = movePosInfo.MoveDir;
            info.PosInfo = movePosInfo;
            Map.ApplyMove(player, new vector2Int(movePosInfo.PosX, movePosInfo.PosY));

            //다른플레이어에게 알려준다.
            S_Move responMovePacket = new S_Move()
            {
                ObjectId = info.ObjectId,
                PosInfo = movePacket.PosInfo,
            };

            BroadCast(responMovePacket);
        }
        public void HandleSkill(Player player, C_Skill skillPacket)
        {
            if (player == null)
                return;

            ObjectInfo info = player.Info;
            if (info.PosInfo.State != CreatureState.Idle)
                return;

            info.PosInfo.State = CreatureState.Skill;

            S_Skill skill = new S_Skill() { Info = new SkillInfo() };
            skill.ObjectId = info.ObjectId;
            skill.Info.SkillId = skillPacket.Info.SkillId;
            BroadCast(skill);

            //TODO ㅡ킬 사용 가능 여부 확인
            Skill skillData = null;
            if (DataManager.SkillDict.TryGetValue(skillPacket.Info.SkillId, out skillData) == false)
                return;

            switch (skillData.skillType)
            {
                case SkillType.SkillNone:
                    break;
                case SkillType.SkillAuto:
                    {
                        //TODO 데미지
                        vector2Int skillPos = player.GetFrontCellPos(info.PosInfo.MoveDir);

                        Player target = Map.Find(skillPos) as Player;
                        if (target != null)
                        {
                            Console.WriteLine(target.Info.Name + "Hit");
                        }
                    }
                    break;
                case SkillType.SkillProjectile:
                    {
                        //TODO 다른스킬
                        Arrow arrow = ObjectManager.Instance.Add<Arrow>();

                        if (arrow == null)
                            return;

                        arrow.Owner = player;
                        arrow.Data = skillData;

                        arrow.PosInfo.State = CreatureState.Moving;
                        arrow.PosInfo.MoveDir = player.PosInfo.MoveDir;
                        arrow.PosInfo.PosX = player.PosInfo.PosX;
                        arrow.PosInfo.PosY = player.PosInfo.PosY;
                        arrow.Speed = skillData.projectile.speed;
                        Push(EnterGame, arrow);
                    }
                    break;
            }
        }
        public void BroadCast(IMessage packet)
        {
            foreach (Player player in _players.Values)
            {
                player.Session.Send(packet);
            }
        }
    }
}
