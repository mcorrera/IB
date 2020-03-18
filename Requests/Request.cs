using System;

namespace Main
{
    internal class Request
    {
        #region Public Fields

        public bool Active;
        public int ConId;
        public int ReqId;
        public int RouteId;
        public DateTime StartTime;
        public DateTime EndTime;
        public string Symbol;

        #endregion Public Fields

        #region Public Constructors

        public Request()
        {
            StartTime = DateTime.Now;
            Active = true;
        }

        public Request(int reqId, int routeId, int conid)
        {
            ReqId = reqId;
            RouteId = routeId;
            ConId = conid;
            Symbol = null;
            Active = true;
            StartTime = DateTime.Now;
        }

        public Request(int reqId, int routeId, int conid, string symbol)
        {
            ReqId = reqId;
            RouteId = routeId;
            ConId = conid;
            Symbol = symbol;
            Active = true;
            StartTime = DateTime.Now;
        }

        #endregion Public Constructors
    }
}