namespace CSN.Uwp.Core
{
    using CSN.Uwp.Strings;
    using CSN.Uwp.Views.Dashboard;
    using CSN.Uwp.Views.Settings;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Media.Animation;
    using Windows.UI.Xaml.Navigation;

    /// <summary>
    /// App shell
    /// </summary>
    public sealed partial class Shell : Page
    {
        #region Fields

        private List<NavigationMenuItem> navigationItems;

        #endregion

        #region Constructor

        public Shell()
        {
            InitializeComponent();
            InitializeNavigation();
        }

        #endregion

        #region Properties

        public Frame RootFrame
            => this.ContentFrame;

        public List<NavigationMenuItem> NavigationItems
        {
            get => this.navigationItems;
            set
            {
                this.RootNavigationView.MenuItems.Clear();
                this.navigationItems = value;
                if (this.navigationItems != null)
                {
                    foreach (NavigationMenuItem item in this.navigationItems)
                    {
                        this.RootNavigationView.MenuItems.Add(new Microsoft.UI.Xaml.Controls.NavigationViewItem()
                        {
                            Content = item.DisplayName,
                            Icon = item.Icon,
                            Tag = item.PageType
                        });
                    }
                }
            }
        }

        public void Navigate(Type pageType, object parameter, bool clearNavigationStack = false, NavigationTransitionInfo navigationInfoOverride = null)
        {
            var navigationMenuItem = NavigationItems.FirstOrDefault(i => i.PageType == pageType);
            if (navigationMenuItem != null)
            {
                DisplayMenuIfRequired();
                this.RootNavigationView.SelectedItem = RootNavigationView.MenuItems.Select(mi => (Type)((Microsoft.UI.Xaml.Controls.NavigationViewItem)mi).Tag == navigationMenuItem.PageType).FirstOrDefault();
            }
            NavigateInternal(pageType, parameter, clearNavigationStack, navigationInfoOverride);
        }

        #endregion

        #region Private methods

        private void DisplayMenuIfRequired()
        {
            if (!this.RootNavigationView.IsPaneVisible)
            {
                this.RootNavigationView.IsPaneToggleButtonVisible = true;
                this.RootNavigationView.IsPaneVisible = true;
                this.RootNavigationView.PaneDisplayMode = Microsoft.UI.Xaml.Controls.NavigationViewPaneDisplayMode.Auto;
            }
        }

        private void InitializeNavigation()
        {
            NavigationItems = new List<NavigationMenuItem>()
            {
                new NavigationMenuItem(LocalizedStrings.GetString("Menu_Dashboard"), typeof(DashboardPage), Symbol.Home),
                new NavigationMenuItem(LocalizedStrings.GetString("Menu_Settings"), typeof(SettingsPage), Symbol.Setting)
            };
            RootNavigationView.ItemInvoked += OnRootNavigationViewItemInvoked;
            RootFrame.Navigated += OnRootFrameNavigated;
        }

        public void NavigateInternal(Type pageType, object parameter, bool clearNavigationStack = false, NavigationTransitionInfo navigationInfoOverride = null)
        {
            this.RootFrame.Navigate(pageType, parameter, navigationInfoOverride);
            if (clearNavigationStack)
            {
                this.RootFrame.BackStack?.Clear();
            }
        }

        private void OnRootNavigationViewItemInvoked(Microsoft.UI.Xaml.Controls.NavigationView sender, Microsoft.UI.Xaml.Controls.NavigationViewItemInvokedEventArgs args)
        {
            var invokedItem = RootNavigationView.SelectedItem as Microsoft.UI.Xaml.Controls.NavigationViewItem;
            if (invokedItem?.Tag != null)
            {
                NavigateInternal(invokedItem?.Tag as Type, null, true, args.RecommendedNavigationTransitionInfo);
            }
        }

        private void OnRootFrameNavigated(object sender, NavigationEventArgs e)
        {
            try
            {
                App.Current.TelemetryClient.TrackPageView(e.SourcePageType.Name);
            }
            catch (Exception ex)
            {
                // Ignore exceptions with telemetry
                Debug.WriteLine($"Telemetry exception on root frame navigated event handler:{ex.Message}");
            }
        }

        #endregion
    }
}
