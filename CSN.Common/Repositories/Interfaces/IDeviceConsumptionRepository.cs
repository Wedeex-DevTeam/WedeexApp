namespace CSN.Common.Repositories.Interfaces
{
    using CSN.Common.Models;
    using System;
    using System.Threading.Tasks;

    public interface IDeviceConsumptionRepository
    {
        #region Properties

        double CommonEffort { get; }

        DeviceConsumption LastDeviceConsumption { get; set; }

        ElectricityNetworkState NetworkState { get; }

        #endregion

        #region Events

        event EventHandler<DeviceConsumptionChangedEventArgs> OnDeviceConsumptionChanged;

        event EventHandler<ElectricityNetworkStateChangedEventArgs> OnNetworkStateChanged;

        #endregion Events

        #region Methods

        Task InitializeAsync();

        Task LoadDeviceConsumptionFromApiAsync();

        Task ReadDeviceConsumptionFor1MinuteAsync();

        #endregion Methods
    }
}