namespace CSN.Uwp.Core
{
    using Microsoft.Toolkit.Mvvm.ComponentModel;
    using Windows.System;
    using Windows.UI.Xaml.Navigation;

    public class CorePageViewModel : ObservableRecipient
    {
        #region Fields

        private bool isLoading;

        #endregion

        #region Properties

        public virtual bool IsLoading
        {
            get => isLoading;
            set => SetProperty(ref isLoading, value);
        }

        #endregion

        #region Public virtual methods

        public virtual void LoadState(object parameter, NavigationMode navigationMode)
        {
            
        }

        public virtual void SaveState(NavigationMode navigationMode)
        {
        }

        #endregion
    }
}
