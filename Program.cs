using System;

namespace Main
{
    public class Program
    {
        #region Public Fields

        public static IB myIB;

        #endregion Public Fields

        #region Public Methods

        public static void Main(string[] args)
        {

            // Third Branch
            myIB = new IB();
            //myIB.myScanner.ScanOn();


            string previousDay = TradingTime.PreviousTradingDay(DateTime.Now).ToString("yyyy-MM-dd");
            string currentDay = DateTime.Now.ToString("yyyy-MM-dd");
            while (TradingTime.IsPreOpening)
            {

                TradingTime.Display();
                MSQL.DailyUpdate();
                DataRequests.GetMostActive(previousDay, 150).Reverse().DisplayScreen();


                Console.WriteLine("MaxActiveRequests= {0} MaxOpenRequests= {1} OldestRequest= {2}", myIB.myRequests.MaxActiveRequests, myIB.myRequests.MaxOpenRequests, myIB.myRequests.OldestRequest);
            }

            while (TradingTime.IsMarketOpen)
            {

                TradingTime.Display();
                MSQL.DailyUpdate();
                DataRequests.GetMostActive(currentDay, 150).Reverse().DisplayScreen();
                Console.WriteLine("MaxActiveRequests= {0} MaxOpenRequests= {1} OldestRequest= {2}", myIB.myRequests.MaxActiveRequests, myIB.myRequests.MaxOpenRequests, myIB.myRequests.OldestRequest);
            }

            while (TradingTime.IsAfterClose)
            {

                TradingTime.Display();
                MSQL.RedoByEditdate();
                MSQL.CheckAllTradingDays();
                MSQL.DeleteVolume(1200);
                MSQL.DailyUpdate();

                DataRequests.GetMostActive(currentDay, 150).Reverse().DisplayScreen();
                Console.WriteLine("MaxActiveRequests= {0} MaxOpenRequests= {1} OldestRequest= {2}", myIB.myRequests.MaxActiveRequests, myIB.myRequests.MaxOpenRequests, myIB.myRequests.OldestRequest);
            }
            myIB.ShutDown();
        }

        #endregion Public Methods
    }
}