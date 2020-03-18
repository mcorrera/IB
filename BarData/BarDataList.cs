using IBApi;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace Main
{
    public class BarDataList
    {
        #region Private Fields

        private int conId;

        private DateTime endTime;

        private List<BarDataItem> items = new List<BarDataItem>();

        private DateTime startTime;

        private string symbol;

        #endregion Private Fields

        #region Public Constructors

        public BarDataList()
        {
            startTime = DateTime.Now;
        }

        public BarDataList(int ConId)
        {
            startTime = DateTime.Now;
            conId = ConId;
        }

        public BarDataList(int ConId, string Symbol)
        {
            startTime = DateTime.Now;
            conId = ConId;
            symbol = Symbol;
        }

        #endregion Public Constructors

        #region Public Properties

        public int ConId { get => (conId); set => conId = value; }
        public DateTime EndDate { get => getMaxDate(); }
        public List<BarDataItem> Items { get => items; set => items = value; }

        /// <summary>
        /// Returns the first trading dar of the BDL
        /// </summary>
        public DateTime StartDate { get => getMinDate(); }

        public DateTime StartEditDate { get => getMinEditDate(); }
        public string Symbol { get => (symbol); }

        #endregion Public Properties

        #region Private Methods

        private bool containsDate(DateTime testDate)
        {
            foreach (BarDataItem item in this.items)
                if (item.Time == testDate.Date)
                    return (true);

            return (false);
        }

        private DateTime convertStringToDT(string dtString)
        {
            //DateTime dateTime = DateTime.Parse(dtString);
            DateTime dateTime = DateTime.ParseExact(dtString, "yyyyMMdd hh:MM:ss", CultureInfo.InvariantCulture);
            return (dateTime);
        }

        private DateTime getMaxDate()
        {
            DateTime maxTime = DateTime.MinValue;
            foreach (BarDataItem bdi in items)
            {
                if (bdi.Time > maxTime)
                    maxTime = bdi.Time;
            }
            return (maxTime);
        }

        private DateTime getMinDate()
        {
            DateTime minTime = DateTime.Today;
            foreach (BarDataItem bdi in items)
            {
                if (bdi.Time < minTime)
                    minTime = bdi.Time;
            }
            return (minTime);
        }

        #endregion Private Methods

        #region Public Methods

        public void Add(int reqId, Bar apiBar)
        {
            BarDataItem myBDI = new BarDataItem(apiBar);
            items.Add(myBDI);
        }

        public void Add(BarDataItem dbBar)
        {
            items.Add(dbBar);
        }

        public long CalcAvgVolume()
        {
            long avgVolume = 0;
            foreach (BarDataItem mBardataItem in items)
                avgVolume += mBardataItem.Volume;
            return (avgVolume / items.Count);
        }

        public void CheckTradingDays()
        {
            DateTime minDate = getMinDate();
            DateTime maxDate = getMaxDate();
            int routeId = 0;

            List<DateTime> tradingDays = TradingTime.TradingCalendar(minDate, maxDate);
            foreach (DateTime item in tradingDays.ToList())
            {
                if (containsDate(item))
                    tradingDays.Remove(item);
            }

            if (tradingDays.Count() != 0)
            {
                foreach (var item in tradingDays)
                {
                    Log.Data(3, string.Format("{0} is Missing date = {1}", DataRequests.GetSymbol(this.ConId), item.Date.ToString("dd/MM/yyyy")));
                    Program.myIB.RequestBarIB(routeId, this.ConId, item.Date);
                    Thread.Sleep(Program.myIB.myRequests.DelayTime);
                }
            }
        }

        public void Clear()
        {
            this.items.Clear();
            this.conId = 0;
            this.symbol = null;
        }

        public void DisplayConsole()

        {
            foreach (BarDataItem mBarDataItem in items)
            {
                Log.Data(3, string.Format("Time= {0:MM/dd/yyyy} Volume= {1,-8} Open= {2:0.00} High={3:0.00} Low= {4:0.00} Close={5:0.00}",
                    mBarDataItem.Time, mBarDataItem.Volume, mBarDataItem.Open, mBarDataItem.High, mBarDataItem.Low, mBarDataItem.Close));
            }
            Log.Data(3, string.Format("Total Displayed = {0}", Items.Count));
        }

        public DateTime getMinEditDate()
        {
            DateTime minTime = DateTime.Today;
            foreach (BarDataItem bdi in items)
            {
                if (bdi.EditTime < minTime)
                    minTime = bdi.EditTime;
            }
            return (minTime);
        }

        public int getNumBadEditDates(DateTime startDate, DateTime endDate, DateTime cutoffDate)
        {
            int numBadEditDates = 0;

            foreach (BarDataItem bdi in items)
                if (bdi.Time >= startDate && bdi.Time <= endDate)
                {
                    if (bdi.EditTime < cutoffDate)
                        numBadEditDates++;
                }
            return (numBadEditDates);
        }

        public BarDataList Trim(DateTime startDate, DateTime endDate)
        {
            foreach (BarDataItem bdi in items.ToList())
            {
                if (bdi.Time < startDate || bdi.Time > endDate)
                    items.Remove(bdi);
            }
            return (this);
        }

        #endregion Public Methods
    }
}