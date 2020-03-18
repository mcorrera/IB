using System;
using System.IO;

namespace Main
{
    public class Log
    {
        #region Private Fields

        private static StreamWriter errorFile, infoFile, dataFile;
        private static string myDocumentsPath;

        #endregion Private Fields

        #region Public Methods

        /// <summary>
        /// Write the Data string to the appropriate output
        /// </summary>
        /// <param name="output">0=None,1=Screen,2=File,3=Both</param>
        /// <param name="dataString">Formated message</param>
        public static void Data(int output, string dataString)
        {
            switch (output)
            {
                case 0:
                    // Do not display any Data message
                    break;

                case 1:
                    // Send the Data message to the console
                    Console.WriteLine(dataString);
                    break;

                case 2:
                    // Send the Data message to the file
                    dataFile.WriteLine(dataString);
                    break;

                case 3:
                    // Send the Data message to bolth the console and the file
                    Console.WriteLine(dataString);
                    dataFile.WriteLine(dataString);
                    break;

                default:
                    // Unknown switch choice
                    Error(3, string.Format("Error - Unknown DataLog Switch : {0}", output));
                    break;
            }
            dataFile.Flush();
        }

        /// <summary>
        /// Write the Error string to the appropriate output
        /// </summary>
        /// <param name="output">0=None,1=Screen,2=File,3=Both</param>
        /// <param name="dataString">Formated message</param>
        public static void Error(int output, string errorString)
        {
            switch (output)
            {
                case 0:
                    // Do not display any Error message
                    break;

                case 1:
                    // Send the Error message to the console
                    Console.WriteLine(errorString);
                    break;

                case 2:
                    // Send the Error message to the file
                    errorFile.WriteLine(errorString);
                    break;

                case 3:
                    // Send the Error message to both the console and the file
                    Console.WriteLine(errorString);
                    errorFile.WriteLine(errorString);
                    break;

                default:
                    // Unknown switch choice
                    Error(3, string.Format("Error - Unknown ErrorLog Switch : {0}", output));
                    break;
            }
            errorFile.Flush();
        }

        /// <summary>
        /// Write the Info string to the appropriate output
        /// </summary>
        /// <param name="output">0=None,1=Screen,2=File,3=Both</param>
        /// <param name="dataString">Formated message</param>
        public static void Info(int output, string infoString)
        {
            switch (output)
            {
                case 0:
                    // Do not display any Info message
                    break;

                case 1:
                    // Send the Info message to the console
                    Console.WriteLine(infoString);
                    break;

                case 2:
                    // Send the Info message to the file
                    infoFile.WriteLine(infoString);
                    break;

                case 3:
                    // Send the Info message to bolth the console and the file
                    Console.WriteLine(infoString);
                    infoFile.WriteLine(infoString);
                    break;

                default:
                    // Unknown switch choice
                    Error(3, string.Format("Error - Unknown InfoLog Switch : {0}", output));
                    break;
            }
            infoFile.Flush();
        }

        /// <summary>
        /// Intialize all of the Log files
        /// </summary>
        public static void Start()
        {
            // Create a filename from timestamp
            string time = DateTime.Now.ToString(" HH-mm-ss");

            string errorFileName = "errorlog" + DateTime.Now.Date.ToString("yyyy-MM-dd") + ".txt";
            string infoFileName = "infolog" + DateTime.Now.Date.ToString("yyyy-MM-dd") + ".txt";
            string dataFileName = "datalog" + DateTime.Now.Date.ToString("yyyy-MM-dd") + time + ".txt";

            // Set a variable to the Documents path.
            myDocumentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            // Set a variable to the Stocks path.
            myDocumentsPath = "D:\\OneDrive\\Stocks\\";

            // Use full path to create a handle to the data file
            dataFile = new StreamWriter(Path.Combine(myDocumentsPath, dataFileName), true);
            // Use full path to create a handle to the info file
            infoFile = new StreamWriter(Path.Combine(myDocumentsPath, infoFileName), true);
            // Use full path to create a handle to the error file
            errorFile = new StreamWriter(Path.Combine(myDocumentsPath, errorFileName), true);

            time = DateTime.Now.ToString("HH:mm:ss");
            errorFile.WriteLine("Start Time {0}", time);
            infoFile.WriteLine("Start Time {0}", time);
        }

        /// <summary>
        /// Close all of the Log Files
        /// </summary>
        public static void Stop()
        {
            errorFile.Close();
            infoFile.Close();
            dataFile.Close();
        }

        #endregion Public Methods
    }
}