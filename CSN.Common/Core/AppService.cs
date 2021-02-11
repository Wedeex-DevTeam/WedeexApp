using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.AppService;
using Windows.Foundation;

namespace CSN.Common.Core
{
    public static class AppService
    {
        #region Events

        public static event TypedEventHandler<object, Tuple<AppServiceTriggerDetails, CancellationTokenSource>> AppServiceConnected;

        #endregion Events

        #region Methods

        public static async Task<T> LaunchFullTrustAndConnect<T>(string groupId, string appServiceName, Func<AppServiceConnection, CancellationTokenSource, Task<T>> exec)
        {
            using (var cts = new CancellationTokenSource())
            {
                var completionSource = new TaskCompletionSource<bool>();

                var appServiceInfoTask =
                     Events.FirstEventOf<object, Tuple<AppServiceTriggerDetails, CancellationTokenSource>>(
                        cts.Token,
                        h => AppService.AppServiceConnected += h,
                        h => AppService.AppServiceConnected -= h,
                        3000,
                        result => result.Item1.Name == appServiceName);

                await FullTrustProcessLauncher.LaunchFullTrustProcessForCurrentAppAsync(groupId);

                var appServiceInfo = await appServiceInfoTask;
                var appServiceConnection = appServiceInfo.Item1.AppServiceConnection;
                using (var serviceCancellation = appServiceInfo.Item2)
                {
                    serviceCancellation.Token.Register(() => completionSource.TrySetResult(false));
                    var result = await exec(appServiceConnection, cts);
                    var _ = await completionSource.Task;
                    return result;
                }
            }
        }

        public static void OnNewAppService(BackgroundActivatedEventArgs args, AppServiceTriggerDetails details)
        {
            if (details.CallerPackageFamilyName == Package.Current.Id.FamilyName)
            {
                // keep the deferal alive for allowing communications between processes
                var appServiceDeferral = args.TaskInstance.GetDeferral();

                var cts = new CancellationTokenSource();

                //Complete deferal when background process is stopped
                cts.Token.Register(() => appServiceDeferral.Complete());
                args.TaskInstance.Canceled += delegate
                {
                    cts.Cancel();
                };

                AppServiceConnected?.Invoke(null, Tuple.Create(details, cts));
            }
        }

        #endregion Methods
    }
}