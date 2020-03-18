using IBApi;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace Main

{
    /// <summary>
    /// A class that holds all of the methods to access the MSQL database
    /// </summary>

    public class MSQL
    {
        #region Private Methods

        /// <summary>
        /// NOT IMPLIMENTED
        /// </summary>
        /// <param name="ticker"></param>
        /// <returns></returns>
        private static double CalculateChange(string symbol)

        {
            var change = 0.0;
            return (change);
        }

        private static BarDataItem convertToBarDataItem(Bar convertBar)
        {
            BarDataItem tempBDI = new BarDataItem
            (

            convertBar.Time,
            convertBar.Open,
            convertBar.High,
           convertBar.Low,
             convertBar.Close,
            convertBar.Volume,
            convertBar.Count,
             convertBar.WAP

                            );
            return (tempBDI);
        }

        /// <summary>
        /// Returns the number of trading days needed to update a specifiy security
        /// </summary>
        /// <param name="ticker">string</param>
        /// <returns>int</returns>
        private static int DaysToUpdate(string symbol)
        {
            int daysToUpdate = TradingTime.GetNumTradingDays(GetLastTradingDay(symbol), DateTime.Now.Date);
            if (daysToUpdate == 0)
                daysToUpdate = 1;
            return (daysToUpdate);
        }

        /// <summary>
        /// Get the first trading day of a specific security
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns>DateTime</returns>
        private static DateTime GetFirstTradingDay(string symbol)
        {
            string cmdString = "SELECT min(tdate) FROM " + DB.myHistTable + " WHERE ticker='" + symbol + "'";
            object result = DB.ExecuteScalar(cmdString);
            return (Convert.ToDateTime(result));
        }

        /// <summary>
        /// Get the Last trading day of a specific security
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns>DateTime</returns>
        private static DateTime GetLastTradingDay(string symbol)
        {
            string cmdString = "SELECT max(tdate) FROM " + DB.myHistTable + " WHERE ticker='" + symbol + "'";
            object result = DB.ExecuteScalar(cmdString);
            return (Convert.ToDateTime(result));
        }

        private static void ReplaceIBbarDB(string symbol, int conId, BarDataItem bar, double change)

        {
            string editDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string cmdString = "REPLACE INTO " + DB.myHistTable + " VALUES ('" + symbol + "','" + conId + "','" + bar.Time.Date.ToString("yyyy-MM-dd") + "','" +
                bar.Volume + "','" + bar.Open + "','" + bar.High + "','" + bar.Low + "','" + bar.Close + "','" + change + "','" + editDate + "')";

            int returnCode = DB.ExecuteNonQuery(cmdString);
            if (returnCode == 1)
                Log.Data(3, string.Format("Insert  {0,-8} {1,-10} {2,-6} {3,-8} {4,-6} {5,-6} {6,-6} {7,-6}", symbol, conId, bar.Time.Date.ToString("dd/MM/yyyy"), bar.Volume, bar.Open, bar.High, bar.Low, bar.Close, change));
            else
                Log.Data(3, string.Format("Replace {0,-8} {1,-10} {2,-6} {3,-8} {4,-6} {5,-6} {6,-6} {7,-6}", symbol, conId, bar.Time.Date.ToString("dd/MM/yyyy"), bar.Volume, bar.Open, bar.High, bar.Low, bar.Close, change));
        }

        private static void SaveData(string symbol)
        {
            string dataFileName = symbol + ".txt";

            // Set a variable to the Documents path.
            string myDocumentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            // Set a variable to the Stocks path.
            myDocumentsPath = "D:\\OneDrive\\Stocks\\Deleted\\";

            // Use full path to create a handle to the error file
            System.IO.StreamWriter dataFile = new StreamWriter(Path.Combine(myDocumentsPath, dataFileName), false);

            BarDataList mBarDataList = new BarDataList();
            mBarDataList = DataRequests.GetAllBars(symbol);

            foreach (BarDataItem mItem in mBarDataList.Items)
            {
                dataFile.WriteLine(string.Format("{0:MM/dd/yyyy} {1:0.000} {2:0.000} {3:0.000} {4:0.000} {5,-8} {6:yyyyMMdd HH:mm:ss}",
                    mItem.Time, mItem.Open, mItem.High, mItem.Low, mItem.Close, mItem.Volume, mItem.EditTime));
            }
            dataFile.Close();
        }

        #endregion Private Methods

        #region Public Methods

        /// <summary>
        /// Attempts to add a SecurityList to the database
        /// </summary>
        /// <param name="newSecurityList"></param>
        /// <returns></returns>
        public static bool AddSecurity(Security addSecurity)

        {
            string duration = "3 Y";
            string endDate = "";
            int routeId = 1;

            if (!Program.myIB.BlackList.Contains(addSecurity))
            {
                Program.myIB.RequestBarIB(routeId, addSecurity.ConId, addSecurity.Symbol, endDate, duration);
                Log.Info(2, string.Format("End AddSecurity {0}", addSecurity.Symbol));
                Thread.Sleep(Program.myIB.myRequests.DelayTime);
                return (true);
            }
            else
                return (false);
        }

        /// <summary>
        /// Attempts to add a SecurityList to the database
        /// </summary>
        /// <param name="newSecurityList"></param>
        /// <returns></returns>
        public static int AddSecurityList(SecurityList newSecurityList)

        {
            int numberAdded = 0;
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            Log.Info(0, string.Format("Start AddList"));
            foreach (Security addSecurity in newSecurityList.Contracts)
            {
                if (DataRequests.GetSymbol(addSecurity.ConId) == null)
                {
                    if (AddSecurity(addSecurity))
                        numberAdded++;
                }
            }
            stopwatch.Stop();
            Log.Info(0, string.Format("End AddList - Added {0} Time={1}", numberAdded, stopwatch.Elapsed.TotalMilliseconds));
            return (numberAdded);
        }

        public static void CheckAllTradingDays()
        {
            Log.Info(3, string.Format("Start CheckAllTradingDays"));
            foreach (Security tempSecurity in IB.masterSecurityList.Contracts.ToList())
            {
                DataRequests.GetAllBars(tempSecurity.Symbol).CheckTradingDays();
            }
            Log.Info(3, string.Format("End CheckAllTradingDays"));
        }

        public static void DailyUpdate()
        {
            int counter = 1;
            int routeId = 0;
            DateTime startTime = DateTime.Now;
            DateTime currentTime;
            TimeSpan elapsedTime = new TimeSpan();

            string daysToUpdate = "1 D";
            string firstTradingDay = "";
            int numberSecurities = IB.masterSecurityList.Contracts.Count();
            Log.Info(2, string.Format("Start UpdateHist"));

            foreach (Security tempSecurity in IB.masterSecurityList.Contracts.ToList())
            {
                #region Calculate Time Remaining

                currentTime = DateTime.Now;
                elapsedTime = currentTime - startTime;
                string elapsed = string.Format("E={0:D2}:{1:D2}:{2:D2}", elapsedTime.Hours, elapsedTime.Minutes, elapsedTime.Seconds);
                TimeSpan timeLeft = (numberSecurities - counter) * (elapsedTime / counter);
                string Remaining = string.Format("Remaining={0:D2}:{1:D2}:{2:D2}", timeLeft.Hours, timeLeft.Minutes, timeLeft.Seconds);
                double percentCompleted = counter / numberSecurities;

                Log.Data(1, string.Format("{0,-10} {1,-10} ActiveReq={2,2} OpenReq={2,2} " + elapsed + "  " + Remaining, tempSecurity.Symbol, tempSecurity.ConId, Program.myIB.myRequests.NumActiveRequests, Program.myIB.myRequests.NumOpenRequests));
                counter++;

                #endregion Calculate Time Remaining

                daysToUpdate = MSQL.DaysToUpdate(tempSecurity.Symbol).ToString() + " D";
                Program.myIB.RequestBarIB(routeId, tempSecurity.ConId, tempSecurity.Symbol, firstTradingDay, daysToUpdate);
                Thread.Sleep(Program.myIB.myRequests.DelayTime);
            }
            Log.Info(3, string.Format("End UpdateHist Total Time= {0} Time/Rec= {1}", elapsedTime, elapsedTime / numberSecurities));
        }

        /// <summary>
        /// Deletes a Security from the database
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns>Number of records deleted</returns>
        public static int Delete(Security tempSecurity)

        {
            string cmdString = "DELETE FROM " + DB.myHistTable + " WHERE TICKER = '" + tempSecurity.Symbol + "'";
            int numberDeleted = DB.ExecuteNonQuery(cmdString);

            Log.Info(3, string.Format("Delete Symbol = {0} {1} Records Deleted", tempSecurity.Symbol, numberDeleted));
            if (!IB.masterSecurityList.Delete(tempSecurity))
                Log.Error(3, string.Format("Delete from masterSecurityList Failed Conid={0,-10} Symbol={1,-8}", tempSecurity.ConId, tempSecurity.Symbol));
            return (numberDeleted);
        }

        /// <summary>
        /// Deletes a Security from the database if last update in longer than 4 days
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        public static bool DeleteDelay(Security tempSecurity)

        {
            DateTime lastTradingDay = GetLastTradingDay(tempSecurity.Symbol);
            int numberDays = (DateTime.Now - lastTradingDay).Days;

            Log.Info(2, string.Format("DeleteDelay Symbol= {0} Days= {1}", tempSecurity.Symbol, numberDays));

            if (numberDays > 6)
            {
                SaveData(tempSecurity.Symbol);
                if (Delete(tempSecurity) > 0)
                    return (true);
                else
                    Log.Error(3, string.Format("DeleteDelay Failed Conid={0,-10} Symbol={1,-8}", tempSecurity.ConId, tempSecurity.Symbol));
            }
            return (false);
        }

        /// <summary>
        /// Deletes from DB is average daily volume fals below minVolume and saves data to file
        /// </summary>
        /// <param name="minVolume">The minimun volume to delete</param>
        public static void DeleteVolume(int minVolume)

        {
            int count = 0;
            foreach (Security tempSecurity in IB.masterSecurityList.Contracts.ToList())
            {
                int avgVol = DataRequests.GetAverageVolume(tempSecurity.Symbol, 100);
                if ((avgVol < minVolume) && (avgVol != -1))
                {
                    Log.Info(2, string.Format("DeleteVolume Conid={0,-10} Symbol={1,-8} Avg. Vol={2,-8} Count={3,-4}", tempSecurity.ConId, tempSecurity.Symbol, avgVol, count++));
                    SaveData(tempSecurity.Symbol);
                    Delete(tempSecurity);
                }
            }
        }

        public static void RedoByEditdate()
        {
            int counter = 1;
            int numUpdated = 0;
            int routeId = 0;

            DateTime startTime = DateTime.Now;
            DateTime currentTime;
            DateTime cutoffDate;
            TimeSpan elapsedTime;
 
            //foreach (Security tempSecurity in IB.masterSecurityList.Contracts.AsEnumerable().Reverse().ToList())
              
            Log.Info(3, string.Format("Start RedoByEditdate"));
            int numberSecurities = IB.masterSecurityList.Contracts.Count();
            foreach (Security tempSecurity in IB.masterSecurityList.Contracts.ToList())
            {
                #region Calculate Time Remaining

                currentTime = DateTime.Now;
                elapsedTime = currentTime - startTime;
                string Elapsed = string.Format("Elapsed={0:D2}:{1:D2}:{2:D2}", elapsedTime.Hours, elapsedTime.Minutes, elapsedTime.Seconds);
                TimeSpan timeLeft = (numberSecurities - counter) * (elapsedTime / counter);
                string Remaining = string.Format("Remaining={0:D2}:{1:D2}:{2:D2}:{3:D2}", timeLeft.Days, timeLeft.Hours, timeLeft.Minutes, timeLeft.Seconds);
                double percentCompleted = counter / numberSecurities;

                Log.Data(1, string.Format("Symbol={0,-10} ConId={1,-10} requests={2,2}  " + Elapsed + "  " + Remaining, tempSecurity.Symbol, tempSecurity.ConId, Program.myIB.myRequests.NumOpenRequests));
                counter++;

                #endregion Calculate Time Remaining

                BarDataList myBDL = new BarDataList();
                myBDL = DataRequests.GetAllBars(tempSecurity.Symbol);

                foreach (BarDataItem tempBar in myBDL.Items)
                {
                    cutoffDate = tempBar.Time.AddDays(7);
                    if (tempBar.EditTime < cutoffDate && DateTime.Now > cutoffDate)
                    {
                        Program.myIB.RequestBarIB(routeId, myBDL.ConId, tempBar.Time);
                        numUpdated++;
                        Thread.Sleep(Program.myIB.myRequests.DelayTime);
                    }
                }

            }
            Log.Info(3, string.Format("End RedoByEditdate - {0} Updated", numUpdated));
        }

        public static void RedoByYear()
        {
            int counter = 1;
            int routeId = 0;
            int numUpdated = 0;
            DateTime cutoffDate = new DateTime(2015, 1, 1);
            DateTime startDate = new DateTime(2017, 01, 01);
            DateTime endDate = new DateTime(2017, 12, 31);

            DateTime startTime = DateTime.Now;
            DateTime currentTime;
            TimeSpan elapsedTime;

            string duration = TradingTime.GetNumTradingDays(startDate, endDate).ToString() + " D";

            int numberSecurities = IB.masterSecurityList.Contracts.Count();
            Log.Info(3, string.Format("Start RedoByYear"));

            foreach (Security tempSecurity in IB.masterSecurityList.Contracts.ToList())
            {
                #region Calculate Time Remaining

                currentTime = DateTime.Now;
                elapsedTime = currentTime - startTime;
                string elapsed = string.Format("E={0:D2}:{1:D2}:{2:D2}", elapsedTime.Hours, elapsedTime.Minutes, elapsedTime.Seconds);
                TimeSpan timeLeft = (numberSecurities - counter) * (elapsedTime / counter);
                string Remaining = string.Format("Remaining={0:D2}:{1:D2}:{2:D2}", timeLeft.Hours, timeLeft.Minutes, timeLeft.Seconds);
                double percentCompleted = counter / numberSecurities;

                Log.Data(1, string.Format("{0,-10} {1,-10} ActiveReq={2,2} OpenReq={2,2} " + elapsed + "  " + Remaining, tempSecurity.Symbol, tempSecurity.ConId, Program.myIB.myRequests.NumActiveRequests, Program.myIB.myRequests.NumOpenRequests));
                counter++;

                #endregion Calculate Time Remaining

                BarDataList myBDL = new BarDataList();
                myBDL = DataRequests.GetAllBars(tempSecurity.Symbol);

                int numBadEditDates = myBDL.getNumBadEditDates(startDate, endDate, cutoffDate);

                if (numBadEditDates > 14)
                {
                    numUpdated++;
                    Program.myIB.RequestBarIB(routeId, tempSecurity.ConId, tempSecurity.Symbol, endDate.ToString("yyyyMMdd hh:MM:ss"), duration);
                    Log.Info(3, string.Format("RedoByYear Requesting Symbol= {0,-10} Bad Edits={1,-5}", tempSecurity.Symbol, numBadEditDates));
                    Thread.Sleep(Program.myIB.myRequests.DelayTime);
                }
                else
                {
                    Log.Info(3, string.Format("RedoByYear Skipping Symbol={0,-10} Bad Edits={1,-5}", tempSecurity.Symbol, numBadEditDates));
                }
            }
            Log.Info(3, string.Format("End RedoByYear - Updated={0} Total={1}", numUpdated, numberSecurities));
        }

        /// <summary>
        /// Callback function that writes the historical data into the database
        /// </summary>
        /// <param name="conId"></param>
        /// <param name="bar"></param>
        /// <param name="end"></param>
        public static void WriteBarDB(int conId, BarDataItem bar, string symbol, bool end)

        {
            //string today = DateTime.Now.Date.ToString("yyyyMMdd");

            if (symbol == null)
            {
                Log.Error(3, string.Format("WriteBarDB Null Symbol ConId={0}", conId));
                return;
            }

            ReplaceIBbarDB(symbol, conId, bar, 0);
            Log.Info(0, string.Format("End WriteBarDB {0,-8} {1,-10}", symbol, conId));
        }

        /// <summary>
        /// Callback function that writes the historical data into the database
        /// </summary>
        /// <param name="reqId"></param>
        /// <param name="bar"></param>
        /// <param name="end"></param>
        public static void WriteBarListDB(int reqId, int conId, string symbol, BarDataList barDataList, bool end)

        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            foreach (BarDataItem barItem in barDataList.Items)
            {
                WriteBarDB(conId, barItem, symbol, false);
            }
            stopWatch.Stop();
            Log.Info(2, string.Format("End WriteBarListDB {0,-8} {1,-10} Records Writen={2}, Time= {3:F0} Average= {4:F0}", symbol, conId, barDataList.Items.Count, stopWatch.Elapsed.TotalMilliseconds, stopWatch.Elapsed.TotalMilliseconds / barDataList.Items.Count));

            Program.myIB.myRequests.Delete(reqId);
            Program.myIB.myBDL.Clear();
        }

        #endregion Public Methods
    }
}