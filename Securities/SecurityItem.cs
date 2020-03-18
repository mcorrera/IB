namespace Main
{
    public class Security
    {
        #region Public Fields

        public int ConId;

        public SecurityType SecType;

        public string Symbol;

        #endregion Public Fields

        #region Public Constructors

        public Security()
        {
        }

        public Security(string symbol, int conid, SecurityType sectype)
        {
            Symbol = symbol;
            ConId = conid;
            SecType = sectype;
        }

        #endregion Public Constructors

        #region Public Enums

        public enum SecurityType
        {
            STK,    // Stock
            ETF,    // ETF
            IND     // Index
        }

        #endregion Public Enums
    }
}