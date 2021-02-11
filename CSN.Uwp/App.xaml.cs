namespace CSN.Uwp
{
    using CSN.Common.Extensions;
    using CSN.Common.Core;
    using CSN.Common.Messaging;
    using CSN.Uwp.Views.Start;
    using Microsoft.ApplicationInsights;
    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.Toolkit.Mvvm.Messaging;
    using Microsoft.WindowsAzure.Messaging;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using Windows.ApplicationModel;
    using Windows.ApplicationModel.Activation;
    using Windows.ApplicationModel.Core;
    using Windows.Networking.PushNotifications;
    using Windows.UI;
    using Windows.UI.ViewManagement;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using CSN.Uwp.Core;
    using Windows.ApplicationModel.AppService;
    using CSN.Common.Repositories.Interfaces;
    using System.ComponentModel;
    using Windows.Storage;

    sealed partial class App : Application
    {
        #region Fields

        private PushNotificationChannel channel;

        private NotificationHub notificationHub;

        private readonly ISettingsRepository settingsRepository;

        #endregion Fields

        #region Constructor

        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
            Services = IoCInitializer.ConfigureServices();
            settingsRepository = (ISettingsRepository)Services.GetService(typeof(ISettingsRepository));
            if (settingsRepository != null)
            {
                settingsRepository.OnSettingsPropertyChanged += OnSettingsRepositorySettingsPropertyChanged;
            }
            this.UnhandledException += OnAppUnhandledException;
        }

        #endregion Constructor

        #region Overridden methods

        /// <summary>
        /// If the app is closed and something triggers foreground activation, OnLaunched isn't even executed, only OnActivated is executed.
        /// </summary>
        /// <param name="args"></param>
        protected override void OnActivated(IActivatedEventArgs args)
        {
            InitializeApp(args).NotAwaited();
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            InitializeApp(args, args.Arguments).NotAwaited();
        }

        #endregion Overridden methods

        #region Properties

        /// <summary>
        /// Gets the current <see cref="App"/> instance in use
        /// </summary>
        public new static App Current => (App)Application.Current;

        /// <summary>
        /// Gets the <see cref="IServiceProvider"/> instance to resolve application services.
        /// </summary>
        public IServiceProvider Services { get; }

        /// <summary>
        /// Gets App's shell <see cref="Shell"/>
        /// </summary>
        public Shell Shell
            => Window.Current.Content as Shell;

        /// <summary>
        /// Gets App's Telemetry client <see cref="TelemetryClient"/>
        /// </summary>
        public TelemetryClient TelemetryClient { get; private set; }

        #endregion Properties

        #region Methods

        private async Task InitializeApp(IActivatedEventArgs e, string arguments = "")
        {
            if (Shell == null)
            {
                InitializeUI();
                await InitializeCoreLogicAsync();

                Window.Current.Content = new Shell();

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    // TODO: Load previous state of suspended app
                }
            }

            Frame rootFrame = Shell.RootFrame;

            if (rootFrame.Content == null)
            {
                Shell.Navigate(typeof(StartPage), arguments);
            }

            Window.Current.Activate();
        }

        private async Task InitializeCoreLogicAsync()
        {
            await CoreConfigurationManager.Instance.InitAsync();
            InitializeTelemetryContexts();
            await InitNotificationsAsync();
        }

        private void InitializeTelemetryContexts()
        {
            try
            {
                TelemetryClient = new TelemetryClient(new TelemetryConfiguration(CoreConfigurationManager.Instance.Configuration.TelemetryInstrumentationKey));
                if (TelemetryClient != null)
                {
                    if (string.IsNullOrEmpty(settingsRepository.UserTelemetryID))
                    {
                        settingsRepository.UserTelemetryID = Guid.NewGuid().ToString();
                    }
                    TelemetryClient.Context.Session.Id = Guid.NewGuid().ToString();
                    TelemetryClient.Context.User.Id = settingsRepository.UserTelemetryID;
                }
            }
            catch
            {
                // Ignore telemetry exception
            }
        }

        protected override void OnBackgroundActivated(BackgroundActivatedEventArgs args)
        {
            base.OnBackgroundActivated(args);

            if (args.TaskInstance.TriggerDetails is AppServiceTriggerDetails details)
            {
                AppService.OnNewAppService(args, details);
            }
        }

        private void OnSettingsRepositorySettingsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(settingsRepository.EnablePushNotification))
            {
                if (settingsRepository.EnablePushNotification)
                {
                    RegisterNotificationAsync().NotAwaited();
                } 
                else
                {
                    UnregisterPushNotificationsAsync().NotAwaited();
                }
            }
        }

        private void InitializeUI()
        {
            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
            ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.BackgroundColor = Colors.Transparent;
            titleBar.InactiveBackgroundColor = Colors.Transparent;
            titleBar.ButtonBackgroundColor = Colors.Transparent;
            titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
            titleBar.ButtonHoverBackgroundColor = (Color)Resources["SystemAccentColor"];
        }

        private async Task InitNotificationsAsync()
        {
            try
            {
                if (channel == null && notificationHub == null)
                {
                    channel = await PushNotificationChannelManager.CreatePushNotificationChannelForApplicationAsync();

                    notificationHub = new NotificationHub(CoreConfigurationManager.Instance.Configuration.NotificationHubPath, CoreConfigurationManager.Instance.Configuration.NotificationHubConnectionString);

                    await RegisterNotificationAsync();
                }
            }
            catch
            {
                // Ignore exception on notifications init
            }
        }

        private async Task RegisterNotificationAsync()
        {
            if (channel != null && notificationHub != null)
            {
                var result = await notificationHub.RegisterNativeAsync(channel.Uri);

                if (result.RegistrationId != null)
                {
                    Debug.WriteLine($"Registration successful: {result.RegistrationId}");
                }

                channel.PushNotificationReceived += OnChannelPushNotificationReceived;
            }
        }

        private async Task UnregisterPushNotificationsAsync()
        {
            if (channel != null && notificationHub != null)
            {
                await notificationHub.UnregisterNativeAsync();
            }
        }

        private void OnAppUnhandledException(object sender, Windows.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            TelemetryClient?.TrackException(e.Exception, new Dictionary<string, string>() { { nameof(e.Handled), e.Handled.ToString() } });
        }

        private void OnChannelPushNotificationReceived(PushNotificationChannel sender, PushNotificationReceivedEventArgs args)
        {
            WeakReferenceMessenger.Default.Send(new ElectricityNetworkStateChangedMessage());
        }

        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            // TODO: Save app state here before suspension
            deferral.Complete();
        }

        #endregion Methods
    }
}