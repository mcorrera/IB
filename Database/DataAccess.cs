using MySql.Data.MySqlClient;
using System;
using System.Diagnostics;

namespace Main
{
    public class DB
    {
        #region Private Fields

        private static string myConnPath = "datasource=localhost;port=3306;username=root;password=lindo123;Allow Zero Datetime=true";

        #endregion Private Fields

        #region Public Fields

        public static string myHistTable = "newschema.historical";

        #endregion Public Fields

        #region Public Methods

        /// <summary>
        /// Execute a ExecuteNonQuery command to the MSQL database
        /// </summary>
        /// <param name="command">string representing the command to be executed</param>
        public static int ExecuteNonQuery(string command)
        {
            MySqlConnection connection = new MySqlConnection(myConnPath);
            connection.Open();

            MySqlCommand cmd = new MySqlCommand(command, connection);
            int result = cmd.ExecuteNonQuery();

            connection.Close();
            return (result);
        }

        /// <summary>
        /// Execute a Reader command to the MSQL database
        /// </summary>
        /// <param name="cmdString"></param>
        /// <returns></returns>
        public static SecurityList ExecuteReader(string cmdString)
        {
            SecurityList mSecurityList = new SecurityList();
            MySqlConnection connection = new MySqlConnection(myConnPath);
            connection.Open();

            MySqlCommand cmd = new MySqlCommand(cmdString, connection);
            MySqlDataReader HistReader = cmd.ExecuteReader();

            while (HistReader.Read())
            {
                Security tempSecurity = new Security
                {
                    ConId = HistReader.GetInt32(0),
                    Symbol = HistReader.GetString(1),
                    SecType = Security.SecurityType.STK
                };

                mSecurityList.Add(tempSecurity);
                Log.Data(0, string.Format("DB.Read Symbol={0,-10} ConId={1,-10} Type={2,-5}", tempSecurity.Symbol, tempSecurity.ConId, tempSecurity.SecType));
            }

            HistReader.Close();
            connection.Close();
            return (mSecurityList);
        }

        /// <summary>
        /// Execute a Scalar command to the MSQL database
        /// </summary>
        /// <param name="command">string representing the command to be executed</param>
        /// <returns>Object</returns>
        public static object ExecuteScalar(string command)
        {
            MySqlConnection connection = new MySqlConnection(myConnPath);
            connection.Open();

            MySqlCommand cmd = new MySqlCommand(command, connection);
            object result = cmd.ExecuteScalar();

            connection.Close();
            return (result);
        }

        public static BarDataList ReadAllBars(string cmdString)
        {
            BarDataList mBarDataList = new BarDataList();
            MySqlConnection connection = new MySqlConnection(myConnPath);
            connection.Open();

            MySqlCommand cmd = new MySqlCommand(cmdString, connection);
            MySqlDataReader dataReader = cmd.ExecuteReader();

            int numBars = 0;
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            // SELECT ticker, tdate, volume, open, high, low, close, editdate FROM historical WHERE ticker = "FB";
            while (dataReader.Read())
            {
                BarDataItem mBarData = new BarDataItem(
                                dataReader.GetString(1),  // Time
                                dataReader.GetDouble(3),  // Open
                                dataReader.GetDouble(4),  // High
                                dataReader.GetDouble(5),  // Low
                                dataReader.GetDouble(6),  // Close
                                dataReader.GetUInt32(2),  // Volume
                                0,  // Other
                                0);  // Other

                mBarData.editTime = dataReader.GetDateTime(7);  // EditTime
                mBarDataList.Add(mBarData);
                numBars++;

                Log.Data(0, string.Format("{0,20} {1,8} {2:0.000} {3:0.000} {4:0.000} {5:0.000} {6:MM/dd/yyyy HH:mm:ss}",
                    mBarData.Time, mBarData.Volume, mBarData.Open, mBarData.High, mBarData.Low, mBarData.Close, mBarData.editTime));
            }

            stopwatch.Stop();
            if (numBars != 0)
            Log.Info(2, string.Format("End ReadAllBars Changed={0} Time/Rec= {1:F}", numBars, stopwatch.Elapsed.TotalMilliseconds / numBars));

            dataReader.Close();
            connection.Close();
            return (mBarDataList);
        }

        /// <summary>
        /// Get a bar of data from the MSQL database
        /// </summary>
        /// <param name="cmdString"></param>
        /// <returns></returns>
        public static BarDataItem ReadBar(string cmdString)
        {
            MySqlConnection connection = new MySqlConnection(myConnPath);
            connection.Open();

            MySqlCommand cmd = new MySqlCommand(cmdString, connection);
            MySqlDataReader dataReader = cmd.ExecuteReader();

            dataReader.Read();

            BarDataItem mBarData = new BarDataItem(
                                dataReader.GetString(1),  //Time
                                dataReader.GetUInt32(2),  // Volume
                                dataReader.GetDouble(3),  // Open
                                dataReader.GetDouble(4),  // High
                                dataReader.GetDouble(5),  // Low
                                dataReader.GetUInt32(6),  // Close
                                0,  // Other
                                0);  // Other

            mBarData.EditTime = dataReader.GetDateTime(7);

            Log.Data(3, string.Format("{0:MM/dd/yyyy} {1:0.000} {2:0.000} {3:0.000} {4:0.000} {5,-8} {6:MM/dd/yyyy HH:mm:ss}",
                mBarData.Time, mBarData.Volume, mBarData.Open, mBarData.High, mBarData.Low, mBarData.Close, mBarData.EditTime));

            dataReader.Close();
            connection.Close();
            return (mBarData);
        }

        /// <summary>
        /// NOT COMPLETED
        /// </summary>
        /// <param name="command">string representing the command to be executed</param>
        public static int WriteBarDataList(BarDataList writeBDL)
        {
            MySqlConnection connection = new MySqlConnection(myConnPath);
            connection.Open();

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            int numBars = 0;

            foreach (var bar in writeBDL.Items)
            {
                string symbol = writeBDL.Symbol;
                int conId = writeBDL.ConId;
                double change = 0.0;
                string editDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                string cmdString = "REPLACE INTO " + DB.myHistTable + " VALUES ('" + symbol + "','" + conId + "','" + bar.Time + "','" +
                    bar.Volume + "','" + bar.Open + "','" + bar.High + "','" + bar.Low + "','" + bar.Close + "','" + change + "','" + editDate + "')";

                MySqlCommand cmd = new MySqlCommand(cmdString, connection);
                int result = cmd.ExecuteNonQuery();

                if (result == 1)
                    Log.Data(3, string.Format("Insert  {0,-8} {1,-10} {2,-6} {3,-8} {4,-6} {5,-6} {6,-6} {7,-6}", symbol, conId, bar.Time.ToString("dd/MM/yyyy"), bar.Volume, bar.Open, bar.High, bar.Low, bar.Close, change));
                else
                    Log.Data(3, string.Format("Replace {0,-8} {1,-10} {2,-6} {3,-8} {4,-6} {5,-6} {6,-6} {7,-6}", symbol, conId, bar.Time.ToString("dd/MM/yyyy"), bar.Volume, bar.Open, bar.High, bar.Low, bar.Close, change));

                numBars++;
            }

            stopwatch.Stop();
            Log.Info(2, string.Format("End WriteBarDataList Changed={0} Time/Rec= {1}", numBars, stopwatch.Elapsed.TotalMilliseconds/numBars));

            connection.Close();
            return (numBars);
        }

        #endregion Public Methods
    }
}