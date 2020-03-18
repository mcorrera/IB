using System;
using System.Collections.Generic;

namespace Main
{
    /// <summary>
    /// A Class to hold all of the IB requests
    /// </summary>
    public class RequestList
    {
        #region Private Fields

        private int currentReqId = 1000;
        private int maxActiveRequests = 0;
        private int maxOpenRequests = 0;
        private TimeSpan oldestRequest = TimeSpan.Zero;
        private List<Request> requestList = new List<Request>();

        #endregion Private Fields

        #region Public Properties

        /// <summary>
        /// Gets the DelayTime so we can throtle requests
        /// </summary>
        public int DelayTime
        {
            get => (2 * (int)Math.Pow(NumActiveRequests, 3));
        }

        /// <summary>
        /// Gets the DelayTimeSlow so we can throtle requests that take long
        /// </summary>
        public int DelayTimeSlow
        {
            get => (10000 * requestList.Count);
        }

        public int MaxActiveRequests
        {
            get => (maxActiveRequests);
        }

        /// <summary>
        /// Gets the maximum number of open requests for this session
        /// </summary>
        public int MaxOpenRequests
        {
            get => (maxOpenRequests);
        }

        /// <summary>
        /// The next available RecId for requests
        /// </summary>
        public int NextReqId
        {
            get => getNextReqId();
        }

        /// <summary>
        /// The number of requests currently Active
        /// </summary>
        public int NumActiveRequests
        {
            get => getActiveRequests();
        }

        /// <summary>
        /// The number of requests currently Open
        /// </summary>
        public int NumOpenRequests
        {
            get => getOpenRequests();
        }

        /// <summary>
        /// The length of time oldest request Open
        /// </summary>
        public TimeSpan OldestRequest
        {
            get => getOldestRequest();
        }

        #endregion Public Properties

        #region Private Methods

        private int getActiveRequests()
        {
            int numActiveRequests = 0;
            foreach (Request request in requestList.ToArray())
            {
                if (request.Active == true)
                    numActiveRequests++;
            }
            return (numActiveRequests);
        }

        /// <summary>
        /// Gets the next available ReqId
        /// </summary>
        /// <returns>ReqId</returns>
        private int getNextReqId()
        {
            return (currentReqId++);
        }

        private TimeSpan getOldestRequest()
        {
            foreach (Request request in requestList.ToArray())
            {
                if (DateTime.Now - request.StartTime > oldestRequest)
                    oldestRequest = DateTime.Now - request.StartTime;
            }
            return (oldestRequest);
        }

        private int getOpenRequests()
        {
            return (requestList.Count);
        }

        #endregion Private Methods

        #region Public Methods

        /// <summary>
        /// Adds a request to the RequestList W/O symbol - NOT RECOMMENDED
        /// </summary>
        /// <param name="reqId">Unique reqId</param>
        /// <param name="routeId">Optional routing code</param>
        /// <param name="conId">conId of the request</param>
        public void Add(int reqId, int routeId, int conId)
        {
            Request addRequest = new Request(reqId, routeId, conId);
            requestList.Add(addRequest);
            if (requestList.Count > maxOpenRequests)
                maxOpenRequests = requestList.Count;
            if (NumActiveRequests > MaxActiveRequests)
                maxActiveRequests = NumActiveRequests;
            Log.Info(0, string.Format("Add Request = {0} Route= {1} ConId= {2} Symbol= {3}", reqId, routeId, conId, null));
        }

        /// <summary>
        /// Adds a request to the RequestList
        /// </summary>
        /// <param name="reqId">Unique reqId</param>
        /// <param name="routeId">Optional routing code</param>
        /// <param name="conId">conId of the request</param>
        /// <param name="symbol">symbol of the request</param>
        public void Add(int reqId, int routeId, int conId, string symbol)
        {
            Request addRequest = new Request(reqId, routeId, conId, symbol);
            requestList.Add(addRequest);
            if (requestList.Count > maxOpenRequests)
                maxOpenRequests = requestList.Count;
            if (NumActiveRequests > MaxActiveRequests)
                maxActiveRequests = NumActiveRequests;
            Log.Info(0, string.Format("Add Request = {0} Route= {1} ConId= {2} Symbol= {3}", reqId, routeId, conId, symbol));
        }

        /// <summary>
        /// Deletes a request from the RequestList
        /// </summary>
        /// <param name="reqId">The unique reqId of the request</param>
        public void Delete(int reqId)
        {
            bool found = false;
            oldestRequest = getOldestRequest();
            foreach (Request request in requestList.ToArray())
            {
                if (request == null)
                    Log.Error(3, string.Format("Error Delete Null Request reqId= {0} conId= {1} symbol= {2}", reqId, GetConId(reqId), GetSymbol(reqId)));
                else
                if (request.ReqId == reqId)
                {
                    requestList.Remove(request);
                    found = true;
                }
            }
            if (!found && reqId >= 1000)
                Log.Error(3, string.Format("Error Delete Request reqId = {0}", reqId));
        }

        /// <summary>
        /// Return the conId of a matching request on the RequestList
        /// </summary>
        /// <param name="reqId">The reqId of the desired request</param>
        /// <returns></returns>
        public int GetConId(int reqId)
        {
            foreach (var request in requestList.ToArray())
            {
                if (request.ReqId == reqId)
                    return (request.ConId);
            }
            Log.Error(3, string.Format("GetConId not Found ReqId={0}", reqId));
            return (0);
        }

        /// <summary>
        /// Return the RouteId of a matching request on the RequestList
        /// </summary>
        /// <param name="reqId">The reqId of the desired request</param>
        /// <returns></returns>
        public int GetRouteId(int reqId)
        {
            foreach (var request in requestList.ToArray())
            {
                if (request.ReqId == reqId)
                    return (request.RouteId);
            }
            Log.Error(3, string.Format("GetRouteId not Found ReqId={0}", reqId));
            return (0);
        }

        /// <summary>
        /// Return the Symbol of a matching request on the RequestList
        /// </summary>
        /// <param name="reqId">The reqId of the desired request</param>
        /// <returns>Symbol, if not found returns null</returns>
        public string GetSymbol(int reqId)
        {
            foreach (var request in requestList.ToArray())
            {
                if (request.ReqId == reqId)
                    return (request.Symbol);
            }
            Log.Error(3, string.Format("GetSymbol not Found ReqId={0}", reqId));
            return (null);
        }

        /// <summary>
        /// Return the conId of a matching request on the RequestList
        /// </summary>
        /// <param name="reqId">The reqId of the desired request</param>
        /// <returns></returns>
        public bool IsActive(int reqId)
        {
            foreach (var request in requestList.ToArray())
            {
                if (request.ReqId == reqId)
                    return (request.Active);
            }
            Log.Error(3, string.Format("IsActive not Found ReqId={0}", reqId));
            return (false);
        }

        public void Release(int reqId)
        {
            bool found = false;

            foreach (Request request in requestList.ToArray())
            {
                if (request == null)
                    Log.Error(3, string.Format("Error Release Null Request reqId= {0} conId= {1} symbol= {2}", reqId, GetConId(reqId), GetSymbol(reqId)));
                else
                if (request.ReqId == reqId)
                {
                    request.Active = false;
                    request.EndTime = DateTime.Now;
                    found = true;
                }
            }
            if (!found)
                Log.Error(3, string.Format("Error Release Request reqId= {0}", reqId));
        }

        #endregion Public Methods
    }
}