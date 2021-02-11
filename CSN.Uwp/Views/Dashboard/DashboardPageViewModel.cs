namespace CSN.Uwp.Views.Dashboard
{
    using CSN.Common.Extensions;
    using CSN.Common.Models;
    using CSN.Common.Repositories.Implementations;
    using CSN.Common.Repositories.Interfaces;
    using CSN.Uwp.Core;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using Windows.System;
    using Windows.System.Power;
    using Windows.UI.Xaml.Navigation;
    using Microsoft.Toolkit.Mvvm.Input;
    using System.Threading;
    using CSN.Uwp.Strings;

    public class DashboardPageViewModel : CorePageViewModel
    {
        #region Constants

        private const int REFRESH_TIMER_INTERVAL_IN_MINUTES = 1;

        #endregion

        #region Fields

        private readonly IDeviceConsumptionRepository deviceConsumptionRepository;
        private readonly DispatcherQueue dispatcherQueue;
        private readonly Timer refreshGlobalInformationsTimer;
        private readonly ScheduledTaskService scheduledTaskService;
        private readonly ISettingsRepository settingsRepository;
        private double batteryChargeConsumption;
        private BatteryStatus batteryStatus;
        private double carbonImpact;
        private double commonEffort;

        private double deviceConsumption;
        private bool displayBatteryCongratulation;
        private bool displayBatteryConsumption;
        private bool displayBatteryWarning;
        private ElectricityNetworkState electricityNetworkState;
        private string electricityNetworkStateMarkdownContent;
        private string individualEffort;
        private bool isAccessAuthorized;
        private bool isAppInbackground;
        private bool isConnectingRealtimeMonitor;
        private bool isScheduledTaskRegistered = true;
        private RelayCommand toggleScheduledTaskRegistrationCommand;
        private double userAppsConsumption;

        #endregion Fields

        #region Constructor

        public DashboardPageViewModel(IDeviceConsumptionRepository deviceConsumptionRepository, ScheduledTaskService scheduledTaskService, ISettingsRepository settingsRepository)
        {
            this.deviceConsumptionRepository = deviceConsumptionRepository;
            this.scheduledTaskService = scheduledTaskService;
            this.settingsRepository = settingsRepository;
            dispatcherQueue = DispatcherQueue.GetForCurrentThread();
            refreshGlobalInformationsTimer = new Timer(new TimerCallback(OnRefreshGlobalInformationTimer));
        }

        #endregion Constructor

        #region Properties

        public double BatteryChargeConsumption
        {
            get => batteryChargeConsumption;
            set => SetProperty(ref batteryChargeConsumption, value);
        }

        public BatteryStatus BatteryStatus
        {
            get => batteryStatus;
            set
            {
                SetProperty(ref batteryStatus, value);
                RefreshBatteryWarningDisplay();
            }
        }

        public double CarbonImpact
        {
            get => carbonImpact;
            set => SetProperty(ref carbonImpact, value);
        }

        public double CommonEffort
        {
            get => commonEffort;
            set => SetProperty(ref commonEffort, value);
        }

        public double DeviceConsumption
        {
            get => deviceConsumption;
            set => SetProperty(ref deviceConsumption, value);
        }
        
        public bool DisplayBatteryCongratulation
        {
            get => displayBatteryCongratulation;
            set => SetProperty(ref displayBatteryCongratulation, value);
        }

        public bool DisplayBatteryConsumption
        {
            get => displayBatteryConsumption;
            set => SetProperty(ref displayBatteryConsumption, value);
        }

        public bool DisplayBatteryWarning
        {
            get => displayBatteryWarning;
            set => SetProperty(ref displayBatteryWarning, value);
        }

        public ElectricityNetworkState ElectricityNetworkState
        {
            get => electricityNetworkState;
            set
            {
                SetProperty(ref electricityNetworkState, value);
                ElectricityNetworkStateMarkdownContent = LocalizedStrings.GetString($"NetworkStates_{ElectricityNetworkState}_MarkdownContent");
                RefreshBatteryWarningDisplay();
            }
        }

        public string ElectricityNetworkStateMarkdownContent
        {
            get => electricityNetworkStateMarkdownContent;
            set => SetProperty(ref electricityNetworkStateMarkdownContent, value);
        }

        public string IndividualEffort
        {
            get => individualEffort;
            set => SetProperty(ref individualEffort, value);
        }

        public bool IsAccessAuthorized
        {
            get => isAccessAuthorized;
            set => SetProperty(ref isAccessAuthorized, value);
        }

        public bool IsScheduledTaskRegistered
        {
            get => isScheduledTaskRegistered;
            set => SetProperty(ref isScheduledTaskRegistered, value);
        }

        public RelayCommand ToggleScheduledTaskRegistrationCommand
            => this.toggleScheduledTaskRegistrationCommand ?? (toggleScheduledTaskRegistrationCommand = new RelayCommand(() => this.ToggleScheduledTaskRegistrationAsync().NotAwaited()));

        public double UserAppsConsumption
        {
            get => userAppsConsumption;
            set => SetProperty(ref userAppsConsumption, value);
        }

        #endregion Properties

        #region CorePageViewModel members

        public override void LoadState(object parameter, NavigationMode navigationMode)
        {
            base.LoadState(parameter, navigationMode);
            LoadData();

            App.Current.EnteredBackground += AppEnteredBackground;
            App.Current.LeavingBackground += AppLeavingBackground;
            isAppInbackground = false;
            StartRealtimeEnergyMonitor();
            StartRefreshGlobalInformationsTimer();
        }

        public override void SaveState(NavigationMode navigationMode)
        {
            base.SaveState(navigationMode);
            PowerManager.BatteryStatusChanged -= OnPowerManagerBatteryStatusChanged;
            deviceConsumptionRepository.OnDeviceConsumptionChanged -= OnDeviceConsumptionRepositoryDeviceConsumptionChanged;
            deviceConsumptionRepository.OnNetworkStateChanged -= OnDeviceConsumptionRepositoryNetworkStateChanged;
            App.Current.EnteredBackground -= AppEnteredBackground;
            App.Current.LeavingBackground -= AppLeavingBackground;
            StopRefreshGlobalInformationsTimer();
        }

        private void StartRefreshGlobalInformationsTimer()
        {
            var interval = TimeSpan.FromMinutes(REFRESH_TIMER_INTERVAL_IN_MINUTES);
            refreshGlobalInformationsTimer.Change(interval, interval);
        }

        private void StopRefreshGlobalInformationsTimer()
        {
            var interval = Timeout.Infinite;
            refreshGlobalInformationsTimer.Change(interval, interval);
        }
        #endregion CorePageViewModel members

        #region Methods

        private void AppEnteredBackground(object sender, Windows.ApplicationModel.EnteredBackgroundEventArgs e)
        {
            isAppInbackground = true;
            StopRefreshGlobalInformationsTimer();
        }

        private void AppLeavingBackground(object sender, Windows.ApplicationModel.LeavingBackgroundEventArgs e)
        {
            isAppInbackground = false;
            deviceConsumptionRepository.LoadDeviceConsumptionFromApiAsync().NotAwaited();
            StartRealtimeEnergyMonitor();
            StartRefreshGlobalInformationsTimer();
        }

        private double FormatValue(double value)
            => Math.Round(value, 3);

        private void InitializeDeviceConsumption()
        {
            deviceConsumptionRepository.OnDeviceConsumptionChanged += OnDeviceConsumptionRepositoryDeviceConsumptionChanged;
        }

        private void InitializeElectricityNetworkState()
        {
            RefreshCommonStateAndEffort();
            deviceConsumptionRepository.OnNetworkStateChanged += OnDeviceConsumptionRepositoryNetworkStateChanged;
        }

        private void InitializeScheduledTask()
        {
            this.IsScheduledTaskRegistered = scheduledTaskService.IsEnergyCollectorTaskRegistered;
        }

        private void LoadBatteryInfos()
        {
            BatteryStatus = PowerManager.BatteryStatus;
            PowerManager.BatteryStatusChanged += OnPowerManagerBatteryStatusChanged;
        }

        private void LoadData()
        {
            LoadBatteryInfos();
            this.IsAccessAuthorized = true;
            InitializeScheduledTask();
            InitializeElectricityNetworkState();
            InitializeDeviceConsumption();
        }

        private void OnDeviceConsumptionRepositoryDeviceConsumptionChanged(object sender, DeviceConsumptionChangedEventArgs e)
        {
            dispatcherQueue.TryEnqueue(() =>
            {
                RefreshDeviceConsumption(e.NewValue);
            });
        }

        private void OnDeviceConsumptionRepositoryNetworkStateChanged(object sender, ElectricityNetworkStateChangedEventArgs e)
        {
            dispatcherQueue.TryEnqueue(() =>
            {
                RefreshCommonStateAndEffort();
            });
        }

        private void OnPowerManagerBatteryStatusChanged(object sender, object e)
        {
            dispatcherQueue.TryEnqueue(() =>
            {
                BatteryStatus = PowerManager.BatteryStatus;
            });
        }

        private void OnRefreshGlobalInformationTimer(object state)
        {
            dispatcherQueue.TryEnqueue(() =>
            {
                deviceConsumptionRepository.LoadDeviceConsumptionFromApiAsync().NotAwaited();
            });
        }

        private void RefreshBatteryWarningDisplay()
        {
            DisplayBatteryConsumption = BatteryStatus != BatteryStatus.Discharging && BatteryStatus != BatteryStatus.NotPresent;
            DisplayBatteryWarning = DisplayBatteryConsumption && ElectricityNetworkState != ElectricityNetworkState.Excellent;
            DisplayBatteryCongratulation = BatteryStatus == BatteryStatus.Discharging && BatteryStatus != BatteryStatus.NotPresent && ElectricityNetworkState != ElectricityNetworkState.Excellent;
        }

        private void RefreshCommonStateAndEffort()
        {
            this.ElectricityNetworkState = deviceConsumptionRepository.NetworkState;
            this.CommonEffort = FormatValue(deviceConsumptionRepository.CommonEffort / 1000);
        }
        private void RefreshDeviceConsumption(DeviceConsumption deviceConsumption)
        {
            DeviceConsumption = FormatValue(deviceConsumption.DeviceConsumptionInWatt);
            UserAppsConsumption = FormatValue(deviceConsumption.UserAppsConsumptionInWatt);
            CarbonImpact = FormatValue(deviceConsumption.DeviceImpactInCO2);
            IndividualEffort = deviceConsumption.UserCarbonEffortInCO2g > 0 ? FormatValue(deviceConsumption.UserCarbonEffortInCO2g).ToString() : string.Empty;
            BatteryChargeConsumption = FormatValue(deviceConsumption.BatteryChargeConsumptionInWatt);
        }

        private async void StartRealtimeEnergyMonitor()
        {
            // Starts background power collector for one minute
            if (this.isAppInbackground || this.isConnectingRealtimeMonitor)
            {
                return;
            }

            this.isConnectingRealtimeMonitor = true;
            try
            {
                await deviceConsumptionRepository.ReadDeviceConsumptionFor1MinuteAsync();
            }
            catch
            {
            }
            finally
            {
                this.isConnectingRealtimeMonitor = false;
            }

            StartRealtimeEnergyMonitor();
        }

        private async Task ToggleScheduledTaskRegistrationAsync()
        {
            try
            {
                //bool isTaskRegistered = settingsRepository.EnableDataCollect;

                //settingsRepository.EnableDataCollect = !IsScheduledTaskRegistered;
                await settingsRepository.EnableOrDisableDataCollectAsync(!IsScheduledTaskRegistered);
                IsScheduledTaskRegistered = settingsRepository.EnableDataCollect;
            }
            catch (Exception ex)
            {
                App.Current.TelemetryClient?.TrackException(ex, new Dictionary<string, string>() { { "Handled", true.ToString() }, { "Context", "Scheduled task registration/unregistration" } });
            }
        }

        #endregion Methods
    }
}