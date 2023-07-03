using ServerCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    public class SessionManager
    {
        static SessionManager _instance = new SessionManager();

        public static SessionManager Instance { get { return _instance; } }   

        int _sessionID = 0;

        Dictionary<int, ClientSession> _sessions = new Dictionary<int, ClientSession>();

        object _lock = new object();    

        public ClientSession Generate()
        {
            lock (_lock)
            {
                ClientSession clientSession = new ClientSession();
                int sessionId = ++_sessionID;
                clientSession.SessionID = sessionId;

                _sessions.Add(sessionId, clientSession);

                return clientSession;
            }
        }

        public ClientSession Find(int id)
        {
            lock (_lock)
            {
                ClientSession clientSession = null;
                _sessions.TryGetValue(id, out clientSession);
                return clientSession;

            }
        }

        public void Remove(ClientSession clientSession)
        {
            lock (_lock)
            {
                _sessions.Remove(clientSession.SessionID);

            }
        }
    }
}
