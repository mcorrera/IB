using System;
using System.Collections.Generic;
using System.Linq;

namespace Main
{
    internal class TradingTime
    {



        #region Private Fields

        private static List<DateTime> holidayCalendar = new List<DateTime>();
        private static List<DateTime> tradingCalendar = new List<DateTime>();

        // These need to be adjusted to universal time
        private static DateTime marketOpenTime = default(DateTime).Add(new TimeSpan(9, 30, 0));
        private static DateTime marketCloseTime = default(DateTime).Add(new TimeSpan(16, 00, 0));

        #endregion Private Fields

        #region Public Constructors

        /// <summary>
        /// Initializes all calendars between 1/1/2017 and 12/31/2020
        /// </summary>
        static TradingTime()
        {
            loadHolidays();

            DateTime endDate = new DateTime(2021, 01, 01);
            DateTime currentDate = new DateTime(2017, 01, 01);

            tradingCalendar.Clear();
            while (currentDate < endDate)
            {
                if (IsWeekday(currentDate) && !IsHoliday(currentDate))
                    tradingCalendar.Add(currentDate);

                currentDate = currentDate.AddDays(1);
            }
        }

        #endregion Public Constructors

        #region Public Properties

        public static DateTime HolidayMaxDate { get => holidayCalendar.Max(); }

        public static DateTime HolidayMinDate { get => holidayCalendar.Min(); }

        public static bool IsMarketClosed { get => !IsMarketOpen; }

        public static bool IsMarketOpen { get => (isMarketHours() && IsTradingDay(DateTime.Now)) ? true : false; }

        public static bool IsPreOpening { get => (IsTradingDay(DateTime.Now) && (convertToEST(DateTime.Now).TimeOfDay < marketOpenTime.TimeOfDay)) ? true : false; }

        public static bool IsAfterClose { get => (convertToEST(DateTime.Now).TimeOfDay > marketCloseTime.TimeOfDay) || IsWeekend(DateTime.Now) ? true : false; }

        public static DateTime TradingMaxDate { get => tradingCalendar.Max(); }

        public static DateTime TradingMinDate { get => tradingCalendar.Min(); }

        #endregion Public Properties

        #region Private Methods


        private static DateTime convertToEST(DateTime mDateTime)
        {
            string easternZoneId = "Eastern Standard Time";
            TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById(easternZoneId);

            return (TimeZoneInfo.ConvertTime(mDateTime, easternZone));
        }


        private static bool isValidDate(DateTime testDate)
        {
            if (testDate < TradingMinDate || testDate > TradingMaxDate)
            {
                Log.Error(3, string.Format("Date Out of Range Error = {0}", testDate));
                return (false);
            }
            else return (true);
        }

        /// <summary>
        /// Needs to be adjust to Universal time
        /// </summary>
        /// <returns></returns>
        private static bool isMarketHours()
        {
            if (convertToEST(DateTime.Now).TimeOfDay > marketOpenTime.TimeOfDay && convertToEST(DateTime.Now).TimeOfDay < marketCloseTime.TimeOfDay)
                return (true);
            else
                return (false);
        }

        /// <summary>
        /// Add of of the holidays from 2017 to 2020
        /// </summary>
        private static void loadHolidays()
        {
            holidayCalendar.Clear();

            // Add 2017 Holidays
            holidayCalendar.Add(new DateTime(2017, 01, 02)); // New Years
            holidayCalendar.Add(new DateTime(2017, 01, 16)); // MLK
            holidayCalendar.Add(new DateTime(2017, 02, 20)); // Presidents
            holidayCalendar.Add(new DateTime(2017, 04, 14)); // Good Friday
            holidayCalendar.Add(new DateTime(2017, 05, 29)); // Memorial
            holidayCalendar.Add(new DateTime(2017, 07, 04)); // Independance
            holidayCalendar.Add(new DateTime(2017, 09, 04)); // Labor
            holidayCalendar.Add(new DateTime(2017, 11, 23)); // Thanksgiving
            holidayCalendar.Add(new DateTime(2017, 12, 25)); // Christmas

            // Add 2018 Holidays
            holidayCalendar.Add(new DateTime(2018, 01, 01)); // New Years
            holidayCalendar.Add(new DateTime(2018, 01, 15)); // MLK
            holidayCalendar.Add(new DateTime(2018, 02, 19)); // Presidents
            holidayCalendar.Add(new DateTime(2018, 03, 30)); // Good Friday
            holidayCalendar.Add(new DateTime(2018, 05, 28)); // Memorial
            holidayCalendar.Add(new DateTime(2018, 07, 04)); // Independance
            holidayCalendar.Add(new DateTime(2018, 09, 03)); // Labor
            holidayCalendar.Add(new DateTime(2018, 11, 22)); // Thanksgiving
            holidayCalendar.Add(new DateTime(2018, 12, 05)); // Bush Funeral
            holidayCalendar.Add(new DateTime(2018, 12, 25)); // Christmas

            // Add 2019 Holidays
            holidayCalendar.Add(new DateTime(2019, 01, 01)); // New Years
            holidayCalendar.Add(new DateTime(2019, 01, 21)); // MLK
            holidayCalendar.Add(new DateTime(2019, 02, 18)); // Presidents
            holidayCalendar.Add(new DateTime(2019, 04, 19)); // Good Friday
            holidayCalendar.Add(new DateTime(2019, 05, 27)); // Memorial
            holidayCalendar.Add(new DateTime(2019, 07, 04)); // Independance
            holidayCalendar.Add(new DateTime(2019, 09, 02)); // Labor
            holidayCalendar.Add(new DateTime(2019, 11, 28)); // Thanksgiving
            holidayCalendar.Add(new DateTime(2019, 12, 25)); // Christmas

            // Add 2020 Holidays
            holidayCalendar.Add(new DateTime(2020, 01, 01)); // New Years
            holidayCalendar.Add(new DateTime(2020, 01, 20)); // MLK
            holidayCalendar.Add(new DateTime(2020, 02, 17)); // Presidents
            holidayCalendar.Add(new DateTime(2020, 04, 10)); // Good Friday
            holidayCalendar.Add(new DateTime(2020, 05, 25)); // Memorial
            holidayCalendar.Add(new DateTime(2020, 07, 03)); // Independance
            holidayCalendar.Add(new DateTime(2020, 09, 07)); // Labor
            holidayCalendar.Add(new DateTime(2020, 11, 26)); // Thanksgiving
            holidayCalendar.Add(new DateTime(2020, 12, 25)); // Christmas
        }

        #endregion Private Methods

        #region Public Methods

        public static void Display()
        {
            TimeSpan tchange = new TimeSpan(0, 0, 0, 0);
            DateTime now = DateTime.Now + tchange;
            Console.WriteLine("Now = {0}", now);
            Console.WriteLine("EST = {0}", convertToEST(now));
            Console.WriteLine("Previous Trading Day = {0}", PreviousTradingDay(now).ToString("dd/MM/yyyy"));
            Console.WriteLine("isValidDate = {0}", isValidDate(now));
            Console.WriteLine("isMarketHours = {0}", isMarketHours());
            Console.WriteLine("IsHoliday = {0}", IsHoliday(now));
            Console.WriteLine("IsTradingDay = {0}", IsTradingDay(now));
            Console.WriteLine("IsWeekday = {0}", IsWeekday(now));
            Console.WriteLine("IsWeekend = {0}", IsWeekend(now));
            Console.WriteLine("IsMarketOpen = {0}", IsMarketOpen);
            Console.WriteLine("IsPreOperning = {0}", IsPreOpening);
            Console.WriteLine("IsAfterClose = {0}", IsAfterClose);
            Console.WriteLine("IsMarketClosed = {0}", IsMarketClosed);

            Console.WriteLine("HolidayMaxDate = {0}", HolidayMaxDate.ToString("dd/MM/yyyy"));
            Console.WriteLine("HolidayMinDate = {0}", HolidayMinDate.ToString("dd/MM/yyyy"));
            Console.WriteLine("TradingMaxDate = {0}", TradingMaxDate.ToString("dd/MM/yyyy"));
            Console.WriteLine("TradingMinDate = {0}", TradingMinDate.ToString("dd/MM/yyyy"));
        }


        public static int GetNumTradingDays(DateTime startDate, DateTime endDate)
        {
            List<DateTime> tempCalendar = new List<DateTime>();

            foreach (var item in tradingCalendar)
            {
                if (item >= startDate && item <= endDate)
                    tempCalendar.Add(item);
            }

            return (tempCalendar.Count - 1);
        }

        public static List<DateTime> HolidayCalendar()
        {
            return (holidayCalendar);
        }

        public static List<DateTime> HolidayCalendar(DateTime startDate, DateTime endDate)
        {
            List<DateTime> tempCalendar = new List<DateTime>();
            foreach (var item in holidayCalendar)
            {
                if (item >= startDate && item <= endDate)
                    tempCalendar.Add(item);
            }
            return (tempCalendar);
        }

        /// <summary>
        /// Returns whether testDate is a Holiday
        /// </summary>
        /// <param name="testDate">DateTiMe</param>
        /// <returns>bool</returns>
        public static bool IsHoliday(DateTime testDate)
        {
            foreach (var item in holidayCalendar)
            {
                if (item.Date == testDate.Date)
                    return (true);
            }
            return (false);
        }

        /// <summary>
        /// Returns whether testDate is a Trading Day
        /// </summary>
        /// <param name="testDate">DateTime</param>
        /// <returns>bool</returns>
        public static bool IsTradingDay(DateTime testDate)
        {
            isValidDate(testDate);
            foreach (var item in tradingCalendar)
            {
                if (item.Date == testDate.Date)
                    return (true);
            }
            return (false);
        }

        /// <summary>
        /// Returns whether testDate is Monday to Friday
        /// </summary>
        /// <param name="testDate">DateTime</param>
        /// <returns>bool</returns>
        public static bool IsWeekday(DateTime date)

        {
            if (date.DayOfWeek != DayOfWeek.Sunday && date.DayOfWeek != DayOfWeek.Saturday)
                return (true);
            else
                return (false);
        }

        /// <summary>
        /// Returns whether testDate is a Saturday or Sunday
        /// </summary>
        /// <param name="testDate">DateTime</param>
        /// <returns>bool</returns>
        public static bool IsWeekend(DateTime date)
        {
            if (date.DayOfWeek == DayOfWeek.Sunday || date.DayOfWeek == DayOfWeek.Saturday)
                return (true);
            else
                return (false);
        }

        public static List<DateTime> TradingCalendar()
        {
            return (tradingCalendar);
        }
        /// <summary>
        /// Returns a subset of the TradingCalander
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public static List<DateTime> TradingCalendar(DateTime startDate, DateTime endDate)
        {
            List<DateTime> workingCalendar = new List<DateTime>();
            foreach (var item in tradingCalendar)
            {
                if (item >= startDate && item <= endDate)
                    workingCalendar.Add(item);
            }
            return (workingCalendar);
        }

        public static DateTime PreviousTradingDay(DateTime testDate)
        {
            if (isValidDate(testDate))
            {
                foreach (var day in tradingCalendar.AsEnumerable().Reverse())
                    if (day.Date < testDate.Date)
                        return (day);

                // We should never get here
                Log.Error(3, string.Format("PreviousTradingDay Date not found= {0}", testDate));
                return (TradingMinDate);
            }
            else
            {
                Log.Error(3, string.Format("PreviousTradingDay Invalid Date= {0}", testDate));
                return (TradingMinDate);
            }
        }

        public static string TradingDaysBack(DateTime startDate, DateTime endDate)
        {
            List<DateTime> tempCalendar = new List<DateTime>();

            foreach (var item in tradingCalendar)
            {
                if (item >= startDate && item <= endDate)
                    tempCalendar.Add(item);
            }

            int numTradingDays = tempCalendar.Count;
            if (numTradingDays < 365)
            {
                return (numTradingDays.ToString() + " D");
            }
            else
            {
                int years = (numTradingDays / 365) + 1;
                return (years.ToString() + " Y");
            }
        }

        #endregion Public Methods
    }
}