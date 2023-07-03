using Microsoft.VisualBasic;
using Server.Data;
using Server.Game;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Server
{
    public class serverP
    {
        static Listener _listener = new Listener();
        static List<System.Timers.Timer> _timers = new List<System.Timers.Timer>();
        static void GameLogicTask()
        {
            while (true)
            {
                RoomManager.Instance.Update();
                Thread.Sleep(0);
            }
        }
        //static void TickRoom(GameRoom room, int tick = 100)
        //{
        //    var timer = new System.Timers.Timer();
        //    timer.Interval = tick;
        //    timer.Elapsed += (s, e) => { room.Update(); };
        //    timer.AutoReset = true;
        //    timer.Enabled = true;

        //    _timers.Add(timer);
        //}

        static void Main(string[] args)
        {
            ConfigManager.LoadConfig();
            DataManager.LoadData();

            GameRoom room = RoomManager.Instance.Add(1);
            //TickRoom(room, 100);

            #region netWork
            string hostName = Dns.GetHostName();
            IPHostEntry ipEntry = Dns.GetHostEntry(hostName);
            IPAddress ipAddr = ipEntry.AddressList[1];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

            Console.WriteLine("==========Server OPEN==========");
            Console.WriteLine("Listener....");
            _listener.Init(endPoint, () => { return SessionManager.Instance.Generate(); });

            //Thread t = new Thread(room.Update);
            //t.Start();
            #endregion
            // GameLogic
            Thread.CurrentThread.Name = "GameLogic";
            GameLogicTask();
        }
    }
}
