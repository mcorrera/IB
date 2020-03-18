using System.Collections.Generic;
using System.Threading;

namespace Main
{
    /// <summary>
    /// A class that holds a list of securities and the associated methods
    /// </summary>
    public class SecurityList
    {
        #region Public Fields

        public List<Security> Contracts = new List<Security>();

        #endregion Public Fields

        #region Private Methods

        /// <summary>
        /// Performs a basic check of the Security
        /// </summary>
        /// <param name="tempSecurity"></param>
        /// <returns>Values 0 = OK, 1 symbol = Null, 2 ConId = 0, SecType = null</returns>
        private int checkSecurity(Security tempSecurity)
        {
            bool error = false;
            int errorValue = 0;

            if (tempSecurity.Symbol == null)
            {
                error = true;
                errorValue = 1;
            }

            if (tempSecurity.Symbol == "")
            {
                error = true;
                errorValue = 1;
            }

            if (tempSecurity.ConId == 0)
            {
                error = true;
                errorValue = 2;
            }

            if (error)
            {
                Log.Error(3, string.Format("CheckSecurity Failed Error= {0} Symbol ={1} ConId ={2} SecType ={3}",
                errorValue, tempSecurity.Symbol, tempSecurity.ConId, tempSecurity.SecType));
            }
            return (errorValue);
        }

        /// <summary>
        /// Removes a security from this list
        /// </summary>
        /// <param name="removeSecurity"></param>
        private void deleteSecurity(Security removeSecurity)
        {
            int index = 0;
            int position = 0;

            foreach (Security tempSecurity in Contracts)
            {
                if (tempSecurity.Symbol == removeSecurity.Symbol)
                    position = index;
                if (tempSecurity.ConId == removeSecurity.ConId)
                    position = index;
                index++;
            }
            Contracts.RemoveAt(position);
        }

        #endregion Private Methods

        #region Public Methods

        /// <summary>
        /// Adds a security to this list does not insert duplicates
        /// </summary>
        /// <param name="addSecurity">The security to Add</param>
        public void Add(Security addSecurity)
        {
            if (checkSecurity(addSecurity) != 0)
                return;

            if (Contains(addSecurity) == true)
                return;

            Contracts.Add(addSecurity);
            Log.Error(0, string.Format("ADDED Symbol={0,-10} ConId={1,-10} Type={2,-5}", addSecurity.Symbol, addSecurity.ConId, addSecurity.SecType));
        }

        /// <summary>
        /// Clears all securities from this list
        /// </summary>
        public void Clear()
        {
            Contracts.Clear();
        }

        /// <summary>
        /// A check to see if a security is in this list
        /// </summary>
        /// <param name="containsSecurity"></param>
        /// <returns>Return True if found, false if not found</returns>
        public bool Contains(Security mSecurity)
        {

            foreach (Security tempSecurity in Contracts)
            {
                if (tempSecurity.Symbol == mSecurity.Symbol)
                    return (true);
                if (tempSecurity.ConId == mSecurity.ConId)
                    return (true);
            }
            return (false);
        }

        /// <summary>
        /// Deleted a security from this list
        /// </summary>
        /// <param name="deleteSecurity"></param>
        public bool Delete(Security removeSecurity)
        {
            if (checkSecurity(removeSecurity) != 0)
                return (false);

            if (Contains(removeSecurity) == false)
            {
                Log.Error(3, string.Format("REMOVE Contains Failed Symbol ={0} ConId ={1} SecType ={2}",
    removeSecurity.Symbol, removeSecurity.ConId, removeSecurity.SecType));
                return (false);
            }
            else
            {
                // Delete the security from this list
                deleteSecurity(removeSecurity);
                Log.Data(3, string.Format("REMOVED Symbol ={0}", removeSecurity.Symbol));
                return (true);
            }
        }

        /// <summary>
        /// Displays securities on this list to the console
        /// </summary>
        public void DisplayConsole()
        {
            foreach (Security tempSecurity in Contracts)
            {
                Log.Info(2, string.Format("Symbol = {0,-10} ConId={1,-10}", tempSecurity.Symbol, tempSecurity.ConId));
            }
            Log.Info(2, string.Format("Total Displayed = {0}", Contracts.Count));
        }

        /// <summary>
        /// Display the list on the IB screen
        /// </summary>
        public void DisplayScreen()
        {
            Program.myIB.ibClient.ClientSocket.subscribeToGroupEvents(9003, 4);

            int count = 1;
            foreach (var contract in Contracts.ToArray())
            {
                Log.Info(1, string.Format("Displaying {0,-10}  {1,2}/{2}", contract.Symbol, count++, Contracts.Count));
                Program.myIB.ibClient.ClientSocket.updateDisplayGroup(9003, contract.ConId.ToString());
                Thread.Sleep(15000);
            }

            Program.myIB.ibClient.ClientSocket.unsubscribeFromGroupEvents(9003);
        }

        /// <summary>
        /// Inserts a security to this list at a given position duplicates
        /// </summary>
        /// <param name=""></param>
        public bool Insert(int index, Security insertSecurity)
        {
            bool validInsert = false;
            if (checkSecurity(insertSecurity) != 0)
                return (validInsert);

            //if (Contains(insertSecurity) == true)
            //    return (validInsert);

            if (index <= Contracts.Count)
            {
                Contracts.Insert(index, insertSecurity);
                validInsert = true;
            }
            else
            {
                Contracts.Add(insertSecurity);
            }
            Log.Data(2, string.Format("INSERTED Symbol={0,-10} ConId={1,-10} Type={2,-5} index={3}", insertSecurity.Symbol, insertSecurity.ConId, insertSecurity.SecType, index));
            return (validInsert);
        }

        /// <summary>
        /// Load all of the values from the MSQL database
        /// </summary>
        public void LoadAll()

        {
            this.Contracts = DataRequests.GetAllSecurities().Contracts;
        }

        public SecurityList Reverse()
        {
            this.Contracts.Reverse();
            return (this);
        }

        #endregion Public Methods
    }
}