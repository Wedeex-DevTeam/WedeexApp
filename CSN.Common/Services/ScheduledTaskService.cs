using CSN.Common.Core;
using System;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.Devices.PointOfService;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Text;

namespace CSN.Common.Repositories.Implementations
{
    public class ScheduledTaskService
    {
        #region Methods

        public bool IsEnergyCollectorTaskRegistered
        {
            get => true.Equals(Windows.Storage.ApplicationData.Current.LocalSettings.Values["scheduledTaskRegistered"]);
        }

        public async Task RegisterEnergyCollectorTask()
        {
            var isTaskRegistered =
                await AppService.LaunchFullTrustAndConnect("ECRegister", "EnergyCollectorRegisterTask", async (appServiceConnection, cts) =>
                {
                    var taskRegisteredCompletion = new TaskCompletionSource<bool>();

                    Observable
                       .FromEvent<TypedEventHandler<AppServiceConnection, AppServiceRequestReceivedEventArgs>, AppServiceRequestReceivedEventArgs>(
                           h => new TypedEventHandler<AppServiceConnection, AppServiceRequestReceivedEventArgs>((_sender, result) => h(result))
                           , h => appServiceConnection.RequestReceived += h
                           , h => appServiceConnection.RequestReceived -= h)
                       .Subscribe(messageEvt =>
                           {
                               if (messageEvt.Request.Message.TryGetValue("registerTaskResult", out var result))
                               {
                                   taskRegisteredCompletion.TrySetResult(true.Equals(result));
                               }
                           }, cts.Token);

                    var deviceId = Windows.System.Profile.SystemIdentification.GetSystemIdForPublisher();
                    var valueSet = new ValueSet();
                    valueSet.Add("deviceId", Convert.ToBase64String(deviceId.Id.ToArray()));
                    await appServiceConnection.SendMessageAsync(valueSet);

                    return await taskRegisteredCompletion.Task;
                });

            Windows.Storage.ApplicationData.Current.LocalSettings.Values["scheduledTaskRegistered"] = isTaskRegistered;
        }

        public Task UnregisterEnergyCollectorTask()
        {
            return AppService.LaunchFullTrustAndConnect("ECRegister", "EnergyCollectorRegisterTask", async (appServiceConnection, cts) =>
            {
                var deviceId = Windows.System.Profile.SystemIdentification.GetSystemIdForPublisher();
                var valueSet = new ValueSet();
                valueSet.Add("unregister", true);
                await appServiceConnection.SendMessageAsync(valueSet);
                return 0;
            });
        }

        #endregion Methods
    }
}