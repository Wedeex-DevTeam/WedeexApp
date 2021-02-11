namespace CSN.Uwp.Core
{
    using Windows.System;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Navigation;

    public class CorePage : Page
    {
        #region Properties

        public virtual CorePageViewModel ViewModel 
        { 
            get => (CorePageViewModel)this.DataContext; 
            set => this.DataContext = value; 
        }

        #endregion

        #region Overridden methods

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ViewModel?.LoadState(e.Parameter, e.NavigationMode);
            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            ViewModel?.SaveState(e.NavigationMode);
            base.OnNavigatedFrom(e);
        }

        #endregion
    }
}
