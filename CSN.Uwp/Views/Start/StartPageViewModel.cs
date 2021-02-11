namespace CSN.Uwp.Views.Start
{
    using CSN.Common.Extensions;
    using CSN.Uwp.Core;
    using CSN.Common.Repositories.Interfaces;
    using CSN.Uwp.Views.Dashboard;
    using System.Threading.Tasks;
    using Windows.UI.Xaml.Navigation;

    public class StartPageViewModel : CorePageViewModel
    {
        #region Fields

        private readonly IDeviceConsumptionRepository deviceConsumptionRepository;

        private readonly NavigationService navigationService;

        #endregion

        #region Constructor

        public StartPageViewModel(IDeviceConsumptionRepository deviceConsumptionRepository, NavigationService navigationService)
        {
            this.deviceConsumptionRepository = deviceConsumptionRepository;
            this.navigationService = navigationService;
        }

        #endregion

        #region CorePageViewModel members

        public override void LoadState(object parameter, NavigationMode navigationMode)
        {
            base.LoadState(parameter, navigationMode);
            LoadDataAsync().NotAwaited();
        }

        #endregion

        #region Methods

        private async Task LoadDataAsync()
        {
            await deviceConsumptionRepository.InitializeAsync();
            navigationService.Navigate<DashboardPage>();
        }

        #endregion
    }
}
