namespace CSN.Uwp.Core
{
    using System;
    using Windows.UI.Xaml.Media.Animation;

    public class NavigationService
    {
        #region Public methods

        public void Navigate(Type pageType, object parameter = null, bool clearNavigationStack = false, NavigationTransitionInfo navigationInfoOverride = null)
            => this.NavigateInternal(pageType, parameter, clearNavigationStack, navigationInfoOverride);

        public void Navigate<TPage>(object parameter = null, bool clearNavigationStack = false, NavigationTransitionInfo navigationInfoOverride = null)
            => this.NavigateInternal(typeof(TPage), parameter, clearNavigationStack, navigationInfoOverride);

        #endregion

        #region Private methods

        private void NavigateInternal(Type pageType, object parameter, bool clearNavigationStack, NavigationTransitionInfo navigationInfoOverride)
            => App.Current.Shell.Navigate(pageType, parameter, clearNavigationStack, navigationInfoOverride);

        #endregion
    }
}
