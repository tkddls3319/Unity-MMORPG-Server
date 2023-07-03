using Google.Protobuf.Protocol;
using Server.Game.Object;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml.Linq;

namespace Server.Game
{
    public struct vector2Int
    {
        public int x;
        public int y;

        public vector2Int(int x, int y) { this.x = x; this.y = y; }
        public static vector2Int up { get { return new vector2Int(0, 1); } }
        public static vector2Int down { get { return new vector2Int(0, -1); } }
        public static vector2Int left { get { return new vector2Int(-1, 0); } }
        public static vector2Int right { get { return new vector2Int(1, 0); } }

        public static vector2Int operator +(vector2Int a, vector2Int b)
        {
            return new vector2Int(a.x + b.x, a.y + b.y);
        }
    }
    public class Map
    {
        public int MinX { get; set; }
        public int MaxX { get; set; }
        public int MinY { get; set; }
        public int MaxY { get; set; }

        bool[,] _collision;
        GameObject[,] _objects;
        public bool CanGo(vector2Int cellPos, bool checkObject = true)
        {
            if (cellPos.x < MinX || cellPos.x > MaxX)
                return false;

            if (cellPos.y < MinY || cellPos.y > MaxY)
                return false;

            int x = cellPos.x - MinX;
            int y = MaxY - cellPos.y;
            return !_collision[y, x] && (checkObject || _objects[y, x] == null);
        }

        /// <summary>
        /// 그리드 관리 목적, map에서 실제 이동하게 좌표 변경 함수
        /// </summary>
        /// <param name="player"></param>
        /// <param name="cellPos"></param>
        /// <returns></returns>
        public bool ApplyMove(GameObject gameObject, vector2Int dest)
        {
            ApplyLeave(gameObject);

            if (gameObject.Room == null)
                return false;
            if (gameObject.Room.Map != this)
                return false;

            PositionInfo posInfo = gameObject.PosInfo;

            if (CanGo(dest, true) == false)
                return false;
          
            //이동좌표에 플레이어 등록
            {
                int x = dest.x - MinX;
                int y = MaxY - dest.y;
                _objects[y, x] = gameObject;
            }

            //실제 좌표이동
            {
                posInfo.PosX = dest.x;
                posInfo.PosY = dest.y;
            }
            return true;
        }
        /// <summary>
        /// 원래좌표에 존재하는 오브젝트 삭제
        /// </summary>
        /// <param name="gameObject"></param>
        /// <returns></returns>
        public bool ApplyLeave(GameObject gameObject)
        {
            if (gameObject.Room == null)
                return false;
            if (gameObject.Room.Map != this)
                return false;

            PositionInfo posInfo = gameObject.PosInfo;
            if (posInfo.PosX < MinX || posInfo.PosX > MaxX)
                return false;
            if (posInfo.PosY < MinY || posInfo.PosY > MaxY)
                return false;

            {
                int x = posInfo.PosX - MinX;
                int y = MaxY - posInfo.PosY;

                if (_objects[y, x] == gameObject)
                    _objects[y, x] = null;

                return true;
            }
        }

        /// <summary>
        /// HandleSkill에서 map에서 플레이어를 받아오는 find함수 작성
        /// </summary>
        /// <param name="cellPos"></param>
        /// <returns></returns>
        public GameObject Find(vector2Int cellPos)
        {
            if (cellPos.x < MinX || cellPos.x > MaxX)
                return null;

            if (cellPos.y < MinY || cellPos.y > MaxY)
                return null;

            int x = cellPos.x - MinX;
            int y = MaxY - cellPos.y;
            return _objects[y, x] = null;
        }

        public void LoadMap(int mapId, string path = "../../../../../Common/MapData")
        {
            string mapName = "Map_" + mapId.ToString("000");

            string text = File.ReadAllText($"{path}/{mapName}.txt");

            StringReader reader = new StringReader(text);

            MinX = int.Parse(reader.ReadLine());
            MaxX = int.Parse(reader.ReadLine());
            MinY = int.Parse(reader.ReadLine());
            MaxY = int.Parse(reader.ReadLine());

            int xCount = MaxX - MinX + 1;
            int yCount = MaxY - MinY + 1;
            _collision = new bool[yCount, xCount];
            _objects = new Player[yCount, xCount];

            for (int y = 0; y < yCount; y++)
            {
                string line = reader.ReadLine();
                for (int x = 0; x < xCount; x++)
                {
                    _collision[y, x] = line[x] == '1' ? true : false;
                }
            }
        }
    }
}
