using IBApi;
using System;
using System.Globalization;

namespace Main
{
    //public class BarDataItem : Bar
    public class BarDataItem
    {
        #region Public Fields

        public double Close;

        public int Count;

        public DateTime editTime;

        public double High;

        public double Low;

        public double Open;

        public DateTime Time;

        public long Volume;

        public double Wap;

        #endregion Public Fields

        #region Public Constructors

        //public BarDataItem(string time, double open, double high, double low, double close, long volume, int count, double wap) : base(time, open, high, low, close, volume, count, wap)
        public BarDataItem(string time, double open, double high, double low, double close, long volume, int count, double wap)
        {
            // Time = convertStringToDT(time);
            Time = DateTime.Parse(time);
            Open = open;
            High = high;
            Low = low;
            Close = close;
            Volume = volume;
            Count = count;
            Wap = wap;
        }

        public BarDataItem(Bar ibBar)
        // DateTime dateTime = DateTime.ParseExact(dtString, "yyyyMMdd", CultureInfo.InvariantCulture);
        {
            Time = DateTime.ParseExact(ibBar.Time, "yyyyMMdd", CultureInfo.InvariantCulture);
            Open = ibBar.Open;
            High = ibBar.High;
            Low = ibBar.Low;
            Close = ibBar.Close;
            Volume = ibBar.Volume;
            Count = ibBar.Count;
            Wap = ibBar.WAP;
        }

        #endregion Public Constructors

        #region Public Properties

        public DateTime EditTime
        {
            get => (editTime);
            set => editTime = value;
        }

        #endregion Public Properties

        #region Private Methods

        private DateTime convertStringToDT(string dtString)
        {
            //DateTime dateTime = DateTime.Parse(dtString);
            DateTime dateTime = DateTime.ParseExact(dtString, "yyyyMMdd", CultureInfo.InvariantCulture);
            return (dateTime);
        }

        #endregion Private Methods

        #region Public Methods

        /// <summary>
        /// Converts an IB bar to BarDataItem
        /// </summary>
        /// <param name="bar"></param>
        /// <returns></returns>
        public BarDataItem Convert(Bar bar)
        {
            BarDataItem mBarData = new BarDataItem(bar.Time, bar.Open, bar.High, bar.Low, bar.Close, bar.Volume, 0, 0);
            return (mBarData);
        }

        public void DisplayConsole()
        {
            Log.Data(3, string.Format("Time= {0:MM/dd/yyyy} Volume= {1,-8} Open= {2:0.00} High={3:0.00} Low= {4:0.00} Close={5:0.00}",
                this.Time, this.Volume, this.Open, this.High, this.Low, this.Close));
        }

        public BarDataItem MakeBar(string time, double open, double high, double low, long close, long volume)
        {
            BarDataItem mBarData = new BarDataItem(time, volume, open, high, low, close, 0, 0);
            return (mBarData);
        }

        public BarDataItem MakeBar()
        {
            BarDataItem mBarData = new BarDataItem(null, 0, 0, 0, 0, 0, 0, 0);
            return (mBarData);
        }

        #endregion Public Methods
    }
}