using Google.Protobuf.Protocol;
using Server.Game.Object;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Game
{
    public class ObjectManager
    {
        public static ObjectManager Instance { get; } = new ObjectManager();
        object _lock = new object();
        Dictionary<int, Player> _players = new Dictionary<int, Player>();


        //[1비트사용안함, 7비트 오브젝트타입, 24비트 아이디]
        int _counter = 1;

        public T Add<T>() where T : GameObject, new()
        {
            T gameObject = new T();

            lock (_lock)
            {
                gameObject.Id = GenerateId(gameObject.ObjectType);

                if (gameObject.ObjectType == GameObjectType.Player)
                {
                    _players.Add(gameObject.Id, gameObject as Player);
                }
            }
            return gameObject;
        }
        int GenerateId(GameObjectType type)
        {
            lock (_lock)
            {
                return (int)type << 24 | (_counter++);
            }
        }

        public static GameObjectType GetObjectTypeById(int id)
        {
            int type = (id >> 24) & 127;
            return (GameObjectType)type;
        }

        //public Player Add()
        //{
        //    Player player = new Player();
        //    lock (_lock)
        //    {
        //        //player.Info.ObjectId = _counter;
        //        //_players.Add(_counter, player);
        //        //_counter++;
        //    }
        //    return player;
        //}

        public bool ReMove(int objectID)
        {
            GameObjectType objectType = GetObjectTypeById(objectID);
            lock (_lock)
            {
                if (objectType == GameObjectType.Player)
                    return _players.Remove(objectID);
            }
            return false;
        }

        public Player Find(int objectID)
        {
            GameObjectType objectType = GetObjectTypeById(objectID);
            lock (_lock)
            {
                if (objectType == GameObjectType.Player)
                {
                    Player player = null;

                    if (_players.TryGetValue(objectID, out player))
                        return player;
                }
                return null;
            }
        }
    }
}
