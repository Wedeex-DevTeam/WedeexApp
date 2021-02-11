namespace CSN.Common.Repositories.Interfaces
{
    using System.ComponentModel;
    using System.Threading.Tasks;

    public interface ISettingsRepository
    {
        #region Public Properties

        bool EnableDataCollect { get; set; }

        bool EnablePushNotification { get; set; }

        string UserTelemetryID { get; set; }

        #endregion

        #region Public Events

        event PropertyChangedEventHandler OnSettingsPropertyChanged;

        #endregion Public Events

        #region Public methods

        Task EnableOrDisableDataCollectAsync(bool isDataCollectEnabled);

        #endregion
    }
}
