using System;
using System.Diagnostics;

namespace Main
{
    public class DataRequests
    {
        #region Public Methods

        public static BarDataList GetAllBars(string symbol)

        {
            Log.Data(2, string.Format("Enter GetAllBars {0,-12}", symbol));
            BarDataList mBarDataList = new BarDataList();

            // SELECT ticker, tdate, volume, open, high, low, close, editdate FROM historical WHERE ticker = "FB";
            string cmdString = "SELECT ticker, tdate, volume, open, high, low, close, editdate FROM " + DB.myHistTable + " WHERE ticker='" + symbol + "'";
            mBarDataList = DB.ReadAllBars(cmdString);
            mBarDataList.ConId = GetConId(symbol);
            return (mBarDataList);
        }

        /// <summary>
        /// Load all of the securites from the database into a table
        /// </summary>
        public static SecurityList GetAllSecurities()

        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            Log.Info(0, string.Format("Start GetAllSecurities"));
            SecurityList mSecurityList = new SecurityList();

            string cmdString = "SELECT DISTINCT conid, ticker FROM " + DB.myHistTable;
            mSecurityList = DB.ExecuteReader(cmdString);
            stopwatch.Stop();
            Log.Info(3, string.Format("End GetAllSecurites {0} Securities Loaded Time={1:N2}", mSecurityList.Contracts.Count, stopwatch.Elapsed.TotalMilliseconds / 1000));
            return (mSecurityList);
        }

        /// <summary>
        /// Get the average trading volume for a specific security for given # of days
        /// </summary>
        /// <returns>DataTime</returns>
        public static int GetAverageVolume(string symbol, int limit)
        {
            string cmdString = "SELECT AVG (volume) FROM (SELECT volume FROM " + DB.myHistTable
                + " WHERE ticker='" + symbol + "' ORDER BY tdate DESC LIMIT " + limit.ToString() + ") sub";
            object result = DB.ExecuteScalar(cmdString);
            if (result != DBNull.Value)
                return (Convert.ToInt32(result));
            else
            {
                Log.Info(3, string.Format("GetAverageVolume DBNull symbol= {0} limit= {1}", symbol, limit));
                return (-1);
            }
        }

        public static BarDataItem GetBar(string symbol, string date)

        {
            Log.Data(3, string.Format("Enter GetBar"));

            // SELECT ticker, tdate, volume, open, high, low, close, editdate FROM historical WHERE ticker = "FB" AND tdate = "2019-09-26";
            string cmdString = "SELECT ticker, tdate, volume, open, high, low, close, editdate FROM " + DB.myHistTable + " WHERE tdate='" + date + "' AND ticker='" + symbol + "'";

            return (DB.ReadBar(cmdString));
        }

        public static int GetConId(string symbol)

        {
            foreach (Security tempSecurity in IB.masterSecurityList.Contracts)
            {
                if (tempSecurity.Symbol == symbol)
                    return (tempSecurity.ConId);
            }
            return (0);
        }

        /// <summary>
        /// Get the latest update to the database
        /// </summary>
        /// <returns>DataTime</returns>
        public static DateTime GetLastUpdate()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            string cmdString = "SELECT max(tdate) FROM " + DB.myHistTable;
            object result = DB.ExecuteScalar(cmdString);
            stopwatch.Stop();
            Log.Info(3, string.Format("End GetLastUpdate Time={0:N3}", stopwatch.Elapsed.TotalMilliseconds / 1000));
            return (Convert.ToDateTime(result));
        }

        /// <summary>
        /// Call the DB and return the most active for that day
        /// </summary>
        /// <param name="date">Date</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns></returns>
        public static SecurityList GetMostActive(string date, int limit)

        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            Log.Info(3, string.Format("Start GetMostActives"));
            SecurityList mSecurityList = new SecurityList();

            string cmdString = "SELECT conid, ticker, volume FROM " + DB.myHistTable + " WHERE tdate='" + date + "' ORDER BY volume DESC LIMIT " + limit.ToString();
            mSecurityList = DB.ExecuteReader(cmdString);
            stopwatch.Stop();
            Log.Info(3, string.Format("End GetMostActives Time={0:N3}", stopwatch.Elapsed.TotalMilliseconds / 1000));
            return (mSecurityList);
        }

        /// <summary>
        /// Converts a ConId to Symbol
        /// </summary>
        /// <param name="conId">int</param>
        /// <returns>string</returns>
        public static string GetSymbol(int conId)

        {
            foreach (Security tempSecurity in IB.masterSecurityList.Contracts)
            {
                if (tempSecurity.ConId == conId)
                    return (tempSecurity.Symbol);
            }
            return (null);
        }

        #endregion Public Methods
    }
}