namespace CSN.Uwp.Views.Settings
{
    using CSN.Common.Repositories.Interfaces;
    using CSN.Uwp.Core;
    using Microsoft.Toolkit.Uwp.Helpers;
    using Windows.UI.Xaml.Navigation;

    public class SettingsPageViewModel : CorePageViewModel
    {
        #region Private Fields

        private readonly ISettingsRepository settingsRepository;

        #endregion

        #region Public Constructor

        public SettingsPageViewModel(ISettingsRepository settingsRepository)
        {
            this.settingsRepository = settingsRepository;
        }

        #endregion

        #region Properties

        public bool EnableDataCollect
        {
            get => this.settingsRepository.EnableDataCollect;

            set
            {
                this.settingsRepository.EnableDataCollect = value;
                OnPropertyChanged(nameof(EnableDataCollect));
            }
        }

        public bool EnablePushNotification
        {
            get => this.settingsRepository.EnablePushNotification;

            set
            {
                this.settingsRepository.EnablePushNotification = value;
                OnPropertyChanged(nameof(EnablePushNotification));
            }
        }

        public string ApplicationVersion => $"v{SystemInformation.Instance.ApplicationVersion.ToFormattedString()}";

        #endregion

        #region Public methods

        public override void LoadState(object parameter, NavigationMode navigationMode)
        {
            base.LoadState(parameter, navigationMode);
            settingsRepository.OnSettingsPropertyChanged += OnSettingsRepositorySettingsPropertyChanged;
        }

        public override void SaveState(NavigationMode navigationMode)
        {
            base.SaveState(navigationMode);
            settingsRepository.OnSettingsPropertyChanged -= OnSettingsRepositorySettingsPropertyChanged;
        }

        #endregion

        #region Private methods

        private void OnSettingsRepositorySettingsPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(settingsRepository.EnableDataCollect))
            {
                OnPropertyChanged(nameof(EnableDataCollect));
            }
        }

        #endregion
    }
}
