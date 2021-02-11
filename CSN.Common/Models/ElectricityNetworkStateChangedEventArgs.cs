namespace CSN.Common.Models
{
    public class ElectricityNetworkStateChangedEventArgs
    {
        #region Constructor

        public ElectricityNetworkStateChangedEventArgs(ElectricityNetworkState oldValue, ElectricityNetworkState newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }

        #endregion

        #region Properties

        public ElectricityNetworkState OldValue { get; set; }

        public ElectricityNetworkState NewValue { get; set; }

        #endregion
    }
}