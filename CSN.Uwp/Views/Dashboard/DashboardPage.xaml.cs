namespace CSN.Uwp.Views.Dashboard
{
    using CSN.Common.Extensions;
    using CSN.Common.Repositories.Implementations;
    using CSN.Uwp.Core;
    using System;
    using Windows.System;
    using Windows.UI.Xaml.Navigation;

    /// <summary>
    /// Dashboard page
    /// </summary>
    public sealed partial class DashboardPage : CorePage
    {
        #region Constructor

        public DashboardPage()
        {
            this.InitializeComponent();
            this.DataContext = App.Current.Services.GetService(typeof(DashboardPageViewModel));
        }

        #endregion Constructor

        #region Properties

        public DashboardPageViewModel PageViewModel { get => (DashboardPageViewModel)DataContext; set => DataContext = value; }

        #endregion Properties

        #region Methods

        private void OnMarkdownTextBlockLinkClicked(object sender, Microsoft.Toolkit.Uwp.UI.Controls.LinkClickedEventArgs e)
        {
            if (Uri.TryCreate(e.Link, UriKind.Absolute, out Uri link))
            {
                Launcher.LaunchUriAsync(link).AsTask().NotAwaited();
            }
        }

        #endregion
    }
}