namespace CSN.Common.Models
{
    public class DeviceConsumptionChangedEventArgs
    {
        #region Constructor

        public DeviceConsumptionChangedEventArgs(DeviceConsumption oldValue, DeviceConsumption newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }

        #endregion

        #region Properties

        public DeviceConsumption OldValue { get; set; }

        public DeviceConsumption NewValue { get; set; }

        #endregion
    }
}