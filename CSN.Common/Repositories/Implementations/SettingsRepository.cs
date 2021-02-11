namespace CSN.Uwp.Repositories.Implementations
{
    using CSN.Common.Extensions;
    using CSN.Common.Repositories.Implementations;
    using CSN.Common.Repositories.Interfaces;
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using Windows.Storage;

    public class SettingsRepository : ISettingsRepository
    {
        #region Fields

        private readonly ScheduledTaskService scheduledTaskService;

        #endregion Fields

        #region Constructor

        public SettingsRepository(ScheduledTaskService scheduledTaskService)
        {
            this.scheduledTaskService = scheduledTaskService;
        }

        #endregion Constructor

        #region Properties

        public bool EnableDataCollect
        {
            get => this.TryGetLocalValue(nameof(EnableDataCollect), false);
            set => ToggleScheduledTaskRegistrationAsync(value).NotAwaited();
        }

        public bool EnablePushNotification
        {
            get => this.TryGetLocalValue(nameof(EnablePushNotification), true);
            set => this.TrySetLocalValue(nameof(EnablePushNotification), value);
        }

        public string UserTelemetryID
        {
            get => this.TryGetLocalValue(nameof(UserTelemetryID), string.Empty);
            set => this.TrySetLocalValue(nameof(UserTelemetryID), value);
        }

        #endregion Properties

        #region public events

        public event PropertyChangedEventHandler OnSettingsPropertyChanged;

        #endregion public events

        #region Public methods

        public async Task EnableOrDisableDataCollectAsync(bool isDataCollectEnabled)
        {
            await ToggleScheduledTaskRegistrationAsync(isDataCollectEnabled);
        }

        #endregion Public methods

        #region Private Methods

        private async Task ToggleScheduledTaskRegistrationAsync(bool enableDataCollect)
        {
            try
            {
                this.TrySetLocalValue(nameof(EnableDataCollect), enableDataCollect);
                if (!enableDataCollect && scheduledTaskService.IsEnergyCollectorTaskRegistered)
                {
                    await scheduledTaskService.UnregisterEnergyCollectorTask();
                }
                else if (enableDataCollect && !scheduledTaskService.IsEnergyCollectorTaskRegistered)
                {
                    await scheduledTaskService.RegisterEnergyCollectorTask();
                }
                this.TrySetLocalValue(nameof(EnableDataCollect), scheduledTaskService.IsEnergyCollectorTaskRegistered);
            }
            catch
            {
                // Ignore exception
            }
        }

        private T TryGetLocalValue<T>(string key, T defaultValue)
        {
            try
            {
                object tValue;

                if (!ApplicationData.Current.LocalSettings.Values.TryGetValue(key, out tValue))
                {
                    return defaultValue;
                }

                return (T)tValue;
            }
            catch
            {
                return defaultValue;
            }
        }

        private void TrySetLocalValue<T>(string key, T value)
        {
            try
            {
                if (ApplicationData.Current.LocalSettings.Values.ContainsKey(key))
                {
                    ApplicationData.Current.LocalSettings.Values[key] = value;
                }
                else
                {
                    ApplicationData.Current.LocalSettings.Values.Add(key, value);
                }
                this.OnSettingsPropertyChanged?.Invoke(this, new PropertyChangedEventArgs(key));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        #endregion Private Methods
    }
}