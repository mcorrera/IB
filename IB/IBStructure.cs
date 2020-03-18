using IBApi;
using System;
using System.Threading;

namespace Main

{
    public class IB
    {
        #region Public Fields

        public static SecurityList masterSecurityList = new SecurityList();
        public SecurityList BlackList = new SecurityList();
        public myEWrapper ibClient = new myEWrapper();
        public BarDataList myBDL = new BarDataList();
        public RequestList myRequests = new RequestList();
        public Scanner myScanner = new Scanner();

        #endregion Public Fields

        #region Public Constructors

        /// <summary>
        /// Startup routines to itialize the IB object
        /// </summary>
        public IB()
        {
            // Start the log file
            Log.Start();

            // Connect to the IB system
            Connect();

            // Load the MasterSecurityfrom the DB
            masterSecurityList.LoadAll();
            Thread.Sleep(3000);
        }

        #endregion Public Constructors

        #region Private Methods

        /// <summary>
        /// Connects to the IB machine
        /// </summary>
        private void Connect()
        {
            EReaderSignal readerSignal = ibClient.Signal;
            EClientSocket clientSocket = ibClient.ClientSocket;

            clientSocket.eConnect("127.0.0.1", 7496, 3);

            //Create a reader to consume messages from the TWS. The EReader will consume the incoming messages and put them in a queue
            var reader = new EReader(clientSocket, readerSignal);
            reader.Start();
            //Once the messages are in the queue, an additional thread need to fetch them
            new Thread(() => { while (clientSocket.IsConnected()) { readerSignal.waitForSignal(); reader.processMsgs(); } }) { IsBackground = true }.Start();

            /*************************************************************************************************************************************************/
            /* One (although primitive) way of knowing if we can proceed is by monitoring the order's nextValidId reception which comes down automatically after connecting. */
            /*************************************************************************************************************************************************/
            while (ibClient.NextOrderId <= 0) { }

            Log.Info(2, string.Format("Exiting Connect"));
        }

        /// <summary>
        /// Disconnects from the IB Machine
        /// </summary>
        private void Disconnect()
        {
            // Disconnect from TWS
            ibClient.ClientSocket.eDisconnect();
            Log.Info(2, string.Format("Exit Disconnect"));
        }

        #endregion Private Methods

        #region Public Methods

        /// <summary>
        /// Requests history data from the IB API
        /// </summary>
        /// <param name="reqId"></param>
        /// <param name="histContract"></param>
        /// <param name="endDate">Last Date requested</param>
        /// <param name="duration">How many days back from endDate</param>
        public void AddBar(int reqId, Bar ibBar)
        {
            int conId = myRequests.GetConId(reqId);
            string symbol = myRequests.GetSymbol(reqId);
            if (symbol == null)
                Log.Error(3, string.Format("AddBar Null Symbol ConId= {0}", conId));

            Log.Data(2, string.Format("AddBar {0,-10} {1,-8} {2:MM/dd/yyyy} {3:0.000} {4:0.000} {3:0.000} {5:0.000} {6,-8}",
                conId, symbol, ibBar.Time, ibBar.Open, ibBar.High, ibBar.Low, ibBar.Close, ibBar.Volume));
            myBDL.Add(reqId, ibBar);
        }

        /// <summary>
        /// Called when all of the bars have been received
        /// </summary>
        /// <param name="reqId"></param>
        public void EndHistoricalBar(int reqId)
        {
            myRequests.Release(reqId);
            int conId = myRequests.GetConId(reqId);
            int routeId = myRequests.GetRouteId(reqId);
            string symbol = myRequests.GetSymbol(reqId);
            if (symbol == null)
                Log.Error(3, string.Format("EndHistoricalBar Null Symbol ConId= {0}", conId));
            Security tempSecurity = new Security
            {
                ConId = conId,
                Symbol = symbol,
                SecType = Security.SecurityType.STK
            };

            switch (routeId)
            {
                case 0:
                    // Route = is an update to an existing security in the DB
                    Log.Data(2, string.Format("EndHistoricalBar {0,-10} {1,-8}", conId, symbol));
                    myBDL.Trim(TradingTime.TradingMinDate, TradingTime.TradingMaxDate);
                    MSQL.WriteBarListDB(reqId, conId, symbol, myBDL, true);
                    break;

                case 1:
                    // Route = 1 means this is a new security to the DB
                    Log.Data(3, string.Format("EndHistoricalBar Route 1 {0,-10} {1,-8}", conId, symbol));
                    //Remove any days that our outside our date range
                    myBDL.Trim(TradingTime.TradingMinDate, TradingTime.TradingMaxDate);

                    // Only add if Average Daily Volume is above minimum
                    if (myBDL.CalcAvgVolume() > 2000)
                    {
                        Log.Data(3, string.Format("Added Volume {0,-10} {1,-8} {2,8}", conId, symbol, myBDL.CalcAvgVolume()));
                        MSQL.WriteBarListDB(reqId, conId, symbol, myBDL, true);
                        IB.masterSecurityList.Add(tempSecurity);
                    }
                    else
                    {
                        // Average Daily Volume is too small, do not add to the DB
                        Log.Data(3, string.Format("Skipped Volume {0,-10} {1,-8} {2,8}", conId, symbol, myBDL.CalcAvgVolume()));
                        BlackList.Add(tempSecurity);
                        Program.myIB.myRequests.Delete(reqId);
                        Program.myIB.myBDL.Clear();
                    }
                    break;

                default:
                    Log.Error(3, string.Format("RouteId Error = {0}", routeId));
                    break;
            }
        }

        /// <summary>
        /// Requests history data from the IB API
        /// </summary>
        /// <param name="reqId"></param>
        /// <param name="histContract"></param>
        /// <param name="endDate">Last Date requested</param>
        /// <param name="duration">How many days back from endDate</param>
        public void RequestBarIB(int routeId, int conId, string symbol, string endDate, string duration)
        {
            Contract histContract = new Contract
            {
                Symbol = symbol,
                ConId = conId,
                Exchange = "SMART",
                PrimaryExch = "ISLAND",
                SecType = "STK",
                Currency = "USD"
            };

            Security tempSecurity = new Security
            {
                Symbol = symbol,
                ConId = conId,
                SecType = Security.SecurityType.STK,
            };

            if (!BlackList.Contains(tempSecurity))
            {
                int reqId = myRequests.NextReqId;
                myRequests.Add(reqId, routeId, conId, symbol);
                Program.myIB.ibClient.ClientSocket.reqHistoricalData(reqId, histContract, endDate, duration, "1 day", "TRADES", 1, 1, false, null);
            }
        }

        public void RequestBarIB(int routeId, int conId, DateTime endDate)
        {
            Contract histContract = new Contract
            {
                Symbol = DataRequests.GetSymbol(conId),
                ConId = conId,
                Exchange = "SMART",
                PrimaryExch = "ISLAND",
                SecType = "STK",
                Currency = "USD"
            };

            Security tempSecurity = new Security
            {
                Symbol = DataRequests.GetSymbol(conId),
                ConId = conId,
                SecType = Security.SecurityType.STK,
            };

            if (!BlackList.Contains(tempSecurity))
            {
                int reqId = myRequests.NextReqId;
                myRequests.Add(reqId, routeId, conId, histContract.Symbol);
                string date = endDate.Date.AddDays(1).ToString("yyyyMMdd HH:mm:ss");
                Program.myIB.ibClient.ClientSocket.reqHistoricalData(reqId, histContract, date, "1 D", "1 day", "TRADES", 1, 1, false, null);
            }
        }

        /// <summary>
        /// Shutdown routines for the IB object
        /// </summary>
        public void ShutDown()
        {
            while (myRequests.NumOpenRequests > 0)
            {
                Thread.Sleep(10000);
                Log.Info(3, string.Format("Pausing..."));
            }
            // Turn off scanners
            if (myScanner.scannersOn)
            {
                myScanner.ScanOff();
                Thread.Sleep(1000);
            }

            // Disconnect from the IB system
            Disconnect();
            Thread.Sleep(1000);

            // Close Log files
            Log.Stop();
        }

        #endregion Public Methods
    }
}