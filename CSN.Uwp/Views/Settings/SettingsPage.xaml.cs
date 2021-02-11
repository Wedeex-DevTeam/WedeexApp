namespace CSN.Uwp.Views.Settings
{
    using CSN.Common.Extensions;
    using CSN.Uwp.Core;
    using System;
    using Windows.System;

    /// <summary>
    /// Settings page
    /// </summary>
    public sealed partial class SettingsPage : CorePage
    {
        public SettingsPage()
        {
            this.InitializeComponent();
            this.DataContext = App.Current.Services.GetService(typeof(SettingsPageViewModel));
        }

        #region Properties

        public SettingsPageViewModel PageViewModel
        {
            get => (SettingsPageViewModel)DataContext;
            set => DataContext = value;
        }
        #endregion

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
