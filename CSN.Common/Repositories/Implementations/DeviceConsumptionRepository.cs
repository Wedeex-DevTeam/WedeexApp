namespace CSN.Common.Repositories.Implementations
{
    using CSN.Common.Core;
    using CSN.Common.Extensions;
    using CSN.Common.Messaging;
    using CSN.Common.Models;
    using CSN.Common.Repositories.Interfaces;
    using Microsoft.Toolkit.Mvvm.Messaging;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Reactive.Linq;
    using System.Runtime.InteropServices.WindowsRuntime;
    using System.Threading.Tasks;
    using Windows.ApplicationModel.AppService;
    using Windows.Devices.Power;
    using Windows.Foundation;

    public class DeviceConsumptionRepository : IDeviceConsumptionRepository
    {
        #region Constants and fields

        private const int MAX_POWER_RAW_VALUES_FOR_AVERAGE = 10;

        // Temporary constant used pendant the implementation of a specific Windows API (the value is calculed for laptops)
        private readonly double OS_POWER_IN_WATT = 5.28;

        private static HttpClient httpClient = new HttpClient(new SignatureDelegateHandler());

        private ConsumptionLogResponse lastConsumptionLogResponse;

        private DeviceConsumption lastDeviceConsumption;

        private ElectricityNetworkState networkState;

        private int? lastBatteryChargeRateInMilliwatts;

        private List<long> lastPowerRawValues = new List<long>();

        #endregion Fields

        #region IDeviceConsumptionRepository members

        public event EventHandler<DeviceConsumptionChangedEventArgs> OnDeviceConsumptionChanged;

        public event EventHandler<ElectricityNetworkStateChangedEventArgs> OnNetworkStateChanged;

        public double CommonEffort { get; private set; }

        public DeviceConsumption LastDeviceConsumption
        {
            get => lastDeviceConsumption;
            set
            {
                var previousValue = lastDeviceConsumption;
                lastDeviceConsumption = value;
                OnDeviceConsumptionChanged?.Invoke(this, new DeviceConsumptionChangedEventArgs(previousValue, lastDeviceConsumption));
            }
        }

        public ElectricityNetworkState NetworkState
        {
            get => networkState;
            private set
            {
                var previousValue = networkState;
                networkState = value;
                OnNetworkStateChanged?.Invoke(this, new ElectricityNetworkStateChangedEventArgs(previousValue, networkState));
            }
        }

        public async Task InitializeAsync()
        {
            try
            {
                await LoadDeviceConsumptionFromApiAsync();
            }
            catch
            {
                // Ignore exception from device consumption api (on open source branch)
            }
            WeakReferenceMessenger.Default.Register<ElectricityNetworkStateChangedMessage>(this, this.OnElectricityNetworkStateChangedMessageReceived);
            Battery.AggregateBattery.ReportUpdated += OnAggregateBatteryReportUpdated;
            RefreshBatteryChargeCarbonImpact(Battery.AggregateBattery.GetReport());
        }

        public async Task LoadDeviceConsumptionFromApiAsync()
        {
            try
            {
                if (CoreConfigurationManager.Instance.Configuration == null)
                {
                    await CoreConfigurationManager.Instance.InitAsync();
                }
                var uri = $"{CoreConfigurationManager.Instance.Configuration.ConsumptionSummaryUrl}?id={Uri.EscapeDataString(GetDeviceId())}";
                var consumptionLogResponse = await httpClient.GetFromJsonAsync<ConsumptionLogResponse>(uri);
                if (consumptionLogResponse != null)
                {                                                                                                                                                                                                                                                                                                                                                                                                                      
                    NetworkState = GetNetworkStateFromIndicator(consumptionLogResponse.Indicator);
                    CommonEffort = consumptionLogResponse.GlobalSaving;

                    lastConsumptionLogResponse = consumptionLogResponse;
                }
            }
            catch
            {
            }
        }

        public Task ReadDeviceConsumptionFor1MinuteAsync()
        {
            return AppService.LaunchFullTrustAndConnect("ECRealtime", "EnergyCollectorRealtime", (appServiceConnection, cts) =>
            {
                Observable
                    .FromEvent<TypedEventHandler<AppServiceConnection, AppServiceRequestReceivedEventArgs>, AppServiceRequestReceivedEventArgs>(
                        h => new TypedEventHandler<AppServiceConnection, AppServiceRequestReceivedEventArgs>((_sender, result) => h(result))
                        , h => appServiceConnection.RequestReceived += h
                        , h => appServiceConnection.RequestReceived -= h)
                    .Subscribe(OnRealTimeMessageReceived, cts.Token);

                return Task.FromResult(0);
            });
        }

        private static string GetDeviceId()
        {
            var deviceId = Windows.System.Profile.SystemIdentification.GetSystemIdForPublisher();
            return Convert.ToBase64String(deviceId.Id.ToArray());
        }

        private static double MilliJoulesToW(double milliJoules)
        {
            var powerWh = milliJoules / 1000;

            return powerWh;
        }

        private static double MilliJoulesToWH(double milliJoules)
        {
            var powerWh = milliJoules / 1000 / 3600;

            return powerWh;
        }

        private static double WHTogCo2(double powerWh, double co2GperKWh)
        {
            var mgCo2perh = powerWh * co2GperKWh;
            return mgCo2perh / 1000;
        }

        private void OnAggregateBatteryReportUpdated(Battery sender, object args)
        {
            RefreshBatteryChargeCarbonImpact(Battery.AggregateBattery.GetReport());
        }

        private void OnRealTimeMessageReceived(AppServiceRequestReceivedEventArgs args)
        {
            var deferal = args.GetDeferral();
            try
            {
                if (args.Request.Message.TryGetValue("realtimePower", out var p))
                {
                    var rawP = StorePowerRawValueAndGetAverage((long)p);
                    var powerW = MilliJoulesToW(rawP);
                    var batteryChargingPower = lastBatteryChargeRateInMilliwatts.HasValue && lastBatteryChargeRateInMilliwatts > 0 ? lastBatteryChargeRateInMilliwatts.Value / 1000 : 0.0;
                    var deviceTotalPowerW = MilliJoulesToW(OS_POWER_IN_WATT * 1000 + (double)rawP) + batteryChargingPower;
                    var co2Rate = 78.0; // gCo2/Kwh
                    var userEffort = 0.0;

                    ConsumptionLogResponse lastLogResponse = lastConsumptionLogResponse;

                    if (lastLogResponse != null)
                    {
                        co2Rate = lastLogResponse.CO2Rate;
                        userEffort = lastLogResponse.Saving;
                    }

                    var deviceConsumption = new DeviceConsumption
                    {
                        UserAppsConsumptionInWatt = powerW,
                        DeviceConsumptionInWatt = deviceTotalPowerW,
                        DeviceImpactInCO2 = WHTogCo2(deviceTotalPowerW / 3600, co2Rate) * 1000,
                        BatteryChargeConsumptionInWatt = batteryChargingPower,
                        // Impact in CO2 of the charge of battery has been removed from dashboard but can come back in future
                        //BatteryChargeImpactInCO2 = WHTogCo2(batteryChargingPower, co2Rate) * 1000,
                        UserCarbonEffortInCO2g = (int)(userEffort / 1000.0)
                    };

                    LastDeviceConsumption = deviceConsumption;
                }
            }
            finally
            {
                deferal.Complete();
            }
        }

        private double StorePowerRawValueAndGetAverage(long rawP)
        {
            lastPowerRawValues.Insert(0, rawP);
            if (lastPowerRawValues.Count > MAX_POWER_RAW_VALUES_FOR_AVERAGE)
            {
                lastPowerRawValues.RemoveAt(MAX_POWER_RAW_VALUES_FOR_AVERAGE);
            }
            return lastPowerRawValues.Average();
        }

        #endregion IDeviceConsumptionRepository members

        #region Private methods

        private ElectricityNetworkState GetNetworkStateFromIndicator(string indicator)
        {
            if (!string.IsNullOrEmpty(indicator))
            {
                switch (indicator.ToLower())
                {
                    case "green":
                        return ElectricityNetworkState.Excellent;

                    case "orange":
                        return ElectricityNetworkState.Poor;

                    case "red":
                        return ElectricityNetworkState.Bad;

                    default:
                        break;
                }
            }

            return ElectricityNetworkState.Unknown;
        }

        private void OnElectricityNetworkStateChangedMessageReceived(object recipient, ElectricityNetworkStateChangedMessage message)
        {
            LoadDeviceConsumptionFromApiAsync().NotAwaited();
        }

        private void RefreshBatteryChargeCarbonImpact(BatteryReport batteryReport)
        {
            lastBatteryChargeRateInMilliwatts = batteryReport.ChargeRateInMilliwatts;
        }

        #endregion Private methods
    }
}
