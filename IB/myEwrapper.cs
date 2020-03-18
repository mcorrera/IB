using IBApi;
using Samples;
using System.Threading;

namespace Main
{
    public class myEWrapper : EWrapperImpl
    {
        #region Public Methods

        public override void displayGroupUpdated(int reqId, string contractInfo)
        {
            //Console.WriteLine("displayGroupUpdated. Request: " + reqId + ", ContractInfo: " + contractInfo);
        }

        public override void error(string str)
        {
            Log.Error(3, string.Format("Unhandled Error: " + str + "\n"));
        }

        public override void error(int reqId, int errorCode, string errorMsg)
        {
            string tempSymbol = null;

            // ReqId < 1000 are reserved for systme use
            if (reqId >= 1000)
            {
                tempSymbol = Program.myIB.myRequests.GetSymbol(reqId);
            }

            if (errorCode < 1000 && reqId >= 1000)
                Program.myIB.myRequests.Delete(reqId);

            Security tempSecurity = new Security(tempSymbol, reqId, Security.SecurityType.STK);

            switch (errorCode)
            {
                case 162:
                    // Historical Market Data Service error message:API scanner subscription cancelled 
                    // Historical Market Data Service error message:No market data permissions for ARCAEDGE STK
                    // Historical Market Data Service error message:No historical market data for 1769/STK@NYSENBBO
                    Log.Error(3, string.Format("Code: {0,4} Message: {1} Ticker: {2,-8} Id: {3,-8}", errorCode, errorMsg, tempSymbol, reqId));
                    Program.myIB.BlackList.Add(tempSecurity);

                    break;

                case 165:
                    // Historical Market Data Service query message:no items retrieved
                    Log.Error(3, string.Format("Code: {0,4} Message: {1} Ticker: {2,-8} Id: {3,-8}", errorCode, errorMsg, tempSymbol, reqId));
                    break;

                case 200:
                    // No security definition has been found for the request
                    Log.Error(3, string.Format("Code: {0,4} Message: {1} Ticker: {2,-8} Id: {3,-8}", errorCode, errorMsg, tempSymbol, reqId));
                    break;

                case 321:
                    // Error validating request:-'bS' : cause - Historical data requested duration is invalid. Error valreqIdating request:-'bS' :
                    // cause - Historical data requests for durations longer than 365 days must be made in years.
                    Log.Error(3, string.Format("Code: {0,4} Message: {1} Ticker: {2,-8} Id: {3,-8}", errorCode, errorMsg, tempSymbol, reqId));
                    break;

                case 322:
                    // Error processing request:-'bS' : cause - Only 50 simultaneous API historical data requests allowed. Duplicate ticker ID for API
                    // historical data query
                    Log.Error(3, string.Format("Code: {0,4} Message: {1} Ticker: {2,-8} Id: {3,-8}", errorCode, errorMsg, tempSymbol, reqId));
                    Thread.Sleep(10000);
                    break;

                case 473:
                    //No Financial Instrument defined
                    Log.Error(3, string.Format("Code: {0,4} Message: {1} Ticker: {2,-8} Id: {3,-8}", errorCode, errorMsg, tempSymbol, reqId));
                    break;

                case 504:
                    //Not connected
                    Log.Error(3, string.Format("Code: {0,4} Message: {1} Ticker: {2,-8} Id: {3,-8}", errorCode, errorMsg, tempSymbol, reqId));
                    break;

                case 2103:
                    // Market data farm connection is broken:usfarm
                    Log.Info(3, string.Format("Code: {0,-4} Message: {1}  Id: {2,-8}", errorCode, errorMsg, reqId));
                    break;

                case 2104:
                    // Market data farm connection is OK:usfuture.nj
                    Log.Info(3, string.Format("Code: {0,-4} Message: {1}  Id: {2,-8}", errorCode, errorMsg, reqId));
                    break;

                case 2105:
                    // HMDS data farm connection is broken:euhmds
                    Log.Info(3, string.Format("Code: {0,-4} Message: {1}  Id: {2,-8}", errorCode, errorMsg, reqId));
                    break;

                case 2106:
                    // HMDS data farm connection is OK:ushmds.nj
                    Log.Info(3, string.Format("Code: {0,-4} Message: {1} Id: {2,-8}", errorCode, errorMsg, reqId));
                    break;

                case 2108:
                    // Market data farm connection is inactive but should be available upon demand.hfarm
                    Log.Info(3, string.Format("Code: {0,-4} Message: {1} Id: {2,-8}", errorCode, errorMsg, reqId));
                    break;

                case 2158:
                    // Sec-def data farm connection is OK:secdefil
                    Log.Info(3, string.Format("Code: {0,-4} Message: {1}  Id: {2,-8}", errorCode, errorMsg, reqId));
                    break;

                default:
                    Log.Error(3, string.Format("Default Error Id: {0,-10} Ticker: {1,-8} Code: {2,-4} Msg: {3}", reqId, tempSymbol, errorCode, errorMsg));
                    break;
            }
        }

        public override void historicalData(int reqId, Bar bar)
        {
            //Log.Data(2, string.Format("historicalData  {0,-10} - Time: {1,-10} Open: {2,-6} High: {3,-6} Low: {4,-6} Close: {5,-6} Volume: {6,-8} Count: {7,-6} WAP: {8}",
            //    reqId, bar.Time, bar.Open, bar.High, bar.Low, bar.Close, bar.Volume, bar.Count, bar.WAP));

            Program.myIB.AddBar(reqId, bar);
        }

        public override void historicalDataEnd(int reqId, string startDate, string endDate)
        {
            Log.Data(0, string.Format("historicalDataEnd " + reqId + " from " + startDate + " to " + endDate));
            Program.myIB.EndHistoricalBar(reqId);
        }

        public override void scannerData(int reqId, int rank, ContractDetails contractDetails, string distance, string benchmark, string projection, string legsStr)
        {
            Log.Data(2, string.Format("scannerData reqId ={0,-8} Symbol ={1,-8} Rank ={2,-3}", reqId, contractDetails.Contract.Symbol.ToString(), rank));
            Program.myIB.myScanner.ReceiveScannerData(reqId, rank, contractDetails, false);
        }

        public override void scannerDataEnd(int reqId)
        {
            Log.Data(2, string.Format("ScannerDataEnd. " + reqId));
            Program.myIB.myScanner.ReceiveScannerData(reqId, 0, null, true);
        }

        #endregion Public Methods
    }
}