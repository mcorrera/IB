using IBApi;
using System.Linq;

namespace Main

{
    public class Scanner
    {
        #region Private Fields

        private SecurityList actives = new SecurityList();
        private SecurityList gainers = new SecurityList();
        private SecurityList losers = new SecurityList();

        #endregion Private Fields

        #region Public Fields

        public bool scannersOn = false;

        #endregion Public Fields

        #region Private Methods
        /// <summary>
        /// Adds the scanner TickerList to the database
        /// </summary>
        /// <param name="reqId">The ID of the scanner</param>
        private void writeScanner(int reqId)
        {
            Log.Info(0, string.Format("writeScanner Start " + reqId));

            // Only add the list if we are not busy
            if (Program.myIB.myRequests.NumActiveRequests < 10)
            {
                if (reqId == 300 && actives.Contracts.Count() != 0)
                {
                    MSQL.AddSecurityList(actives);
                    actives.Clear();
                }

                if (reqId == 301 && gainers.Contracts.Count() != 0)
                {
                    MSQL.AddSecurityList(gainers);
                    gainers.Clear();
                }

                if (reqId == 302 && losers.Contracts.Count() != 0)
                {
                    MSQL.AddSecurityList(losers);
                    losers.Clear();
                }
            }
            
            Log.Info(0, string.Format("writeScanner End. " + reqId));
        }

        /// <summary>
        /// Turns off an individual scanner
        /// </summary>
        /// <param name="choice"></param>
        private void ScanEnd(int choice)
        {
            int reqId = -1 ;
            switch (choice)
            {
                case 0:
                    reqId = 300;
                    break;

                case 1:
                    reqId = 301;
                    break;

                case 2:
                    reqId = 302;
                    break;

                default:
                    Log.Error(3, string.Format("ScanEnd Bad Choice= {0}", choice));
                    break;
            }
            Program.myIB.ibClient.ClientSocket.cancelScannerSubscription(reqId);
        }

        /// <summary>
        /// Starts an individual scanner
        /// </summary>
        /// <param name="choice"></param>
        private void ScanStart(int choice)
        {
            ScannerSubscription scanSub = new ScannerSubscription();
            int reqId = -1;

            switch (choice)
            {
                case 0:
                    reqId = 300;
                    scanSub.Instrument = "STK";
                    scanSub.LocationCode = "STK.US.MAJOR";
                    scanSub.ScanCode = "MOST_ACTIVE";
                    scanSub.AbovePrice = 10;
                    scanSub.AboveVolume = 30000;
                    scanSub.NumberOfRows = 50;
                    break;

                case 1:
                    reqId = 301;
                    scanSub.Instrument = "STK";
                    scanSub.LocationCode = "STK.US.MAJOR";
                    scanSub.ScanCode = "TOP_PERC_GAIN";
                    scanSub.AbovePrice = 10;
                    scanSub.AboveVolume = 50000;
                    scanSub.NumberOfRows = 20;
                    break;

                case 2:
                    reqId = 302;
                    scanSub.Instrument = "STK";
                    scanSub.LocationCode = "STK.US.MAJOR";
                    scanSub.ScanCode = "TOP_PERC_LOSE";
                    scanSub.AbovePrice = 10;
                    scanSub.AboveVolume = 50000;
                    scanSub.NumberOfRows = 20;
                    break;

                default:
                    Log.Error(3, string.Format("ScanStart Bad Choice= {0}", choice));
                    break;
            }
            Program.myIB.ibClient.ClientSocket.reqScannerSubscription(reqId, scanSub, null, (string)null);
        }

        #endregion Private Methods

        #region Public Methods

        /// <summary>
        /// Turns off all scanners
        /// </summary>
        public void ScanOff()
        {
            ScanEnd(0);
            ScanEnd(1);
            ScanEnd(2);

            scannersOn = false;
        }

        /// <summary>
        /// Turns on all scanners
        /// </summary>
        public void ScanOn()
        {
            // Turn on the scanners

            scannersOn = true;

            ScanStart(0);
            ScanStart(1);
            ScanStart(2);
        }

        public void ReceiveScannerData(int reqId, int rank, ContractDetails contractDetails, bool end)
        {
            if (!end)
            {
                Security mContract = new Security(contractDetails.Contract.Symbol.ToString(), contractDetails.Contract.ConId, Security.SecurityType.STK);
                switch (reqId)
                {
                    case 300:
                        if (actives.Insert(rank, mContract) == false)
                            Log.Error(3, string.Format("ReceiveScannerData Insert Fail actives ReqId= {0}", reqId));
                        break;

                    case 301:
                        if (gainers.Insert(rank, mContract) == false)
                            Log.Error(3, string.Format("ReceiveScannerData Insert Fail gainers ReqId= {0}", reqId));
                        break;

                    case 302:
                        if (losers.Insert(rank, mContract) == false)
                            Log.Error(3, string.Format("ReceiveScannerData Insert Fail losers ReqId= {0}", reqId));
                        break;

                    default:
                        Log.Error(3, string.Format("ReceiveScannerData Bad ReqId= {0}", reqId));
                        break;
                }
            }
            else
            {
                Log.Data(0, string.Format("ReceiveScannerData End " + reqId));
                writeScanner(reqId);
            }
        }

        #endregion Public Methods
    }
}