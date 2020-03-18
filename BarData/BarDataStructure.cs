using System.Collections.Generic;

namespace Main
{
    /// <summary>
    /// A Class to hold all of the IB data requests
    /// </summary>
    public class BarDataStructure
    {
        #region Private Fields

        private List<BarDataList> BDSList = new List<BarDataList>();
        private int maxLists = 0;

        #endregion Private Fields

        #region Public Properties

        /// <summary>
        /// Gets the maximum number of open requests for this session
        /// </summary>
        public int MaxOpenLists
        {
            get => (maxLists);
        }

        /// <summary>
        /// Gets the number of requests currently open
        /// </summary>
        public int NumOpenLists
        {
            get => (BDSList.Count);
        }

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// Adds a new BDL to the BDSList
        /// </summary>
        /// <param name="reqId">Unique reqId</param>
        /// <param name="routeId">Optional routing code</param>
        /// <param name="conId">conId of the request</param>
        public void Add(int conId)
        {
            BarDataList addBDL = new BarDataList(conId);
            BDSList.Add(addBDL);
            if (BDSList.Count > maxLists)
                maxLists = BDSList.Count;
            Log.Info(0, string.Format("Add BDS = {0}", conId));
        }

        /// <summary>
        /// Adds a request to the RequestList
        /// </summary>
        /// <param name="reqId">Unique reqId</param>
        /// <param name="routeId">Optional routing code</param>
        /// <param name="conId">conId of the request</param>
        /// <param name="symbol">symbol of the request</param>
        public void Add(int conId, string symbol)
        {
            BarDataList addBDL = new BarDataList(conId, symbol);
            BDSList.Add(addBDL);
            if (BDSList.Count > maxLists)
                maxLists = BDSList.Count;
            Log.Info(0, string.Format("Add BDS = {0}", conId));
        }

        /// <summary>
        /// Deletes a request from the RequestList
        /// </summary>
        /// <param name="reqId">The unique reqId of the request</param>
        public void Delete(int conId)
        {
            int count = 0;
            foreach (BarDataList BDL in BDSList.ToArray())
            {
                if (BDL.ConId == conId)
                    BDSList.Remove(BDL);

                count++;
            }
            Log.Info(0, string.Format("Remove Requests = {0}", conId));
        }

        #endregion Public Methods
    }
}