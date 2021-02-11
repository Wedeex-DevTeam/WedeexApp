namespace CSN.Uwp.BackgroundTask
{
    using CSN.Common.Repositories.Implementations;
    using System;
    using Windows.ApplicationModel.Background;
    using Windows.ApplicationModel;
    using System.Threading.Tasks;
    using Windows.Storage;
    using Newtonsoft.Json;
    using System.Net.Http;
    using Windows.UI.WebUI;
    using System.Linq;
    using CSN.Common.Models;
    using System.Diagnostics;

    public sealed class CollectBackgroundTask : IBackgroundTask
    {
        #region Constants and fields

        BackgroundTaskDeferral deferral;

        #endregion

        #region Constructor

        public CollectBackgroundTask()
        {
        }

        #endregion

        #region IBackgroundTask members

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            deferral = taskInstance.GetDeferral();

            //Debugger.Launch();
            try
            {
                var e3log = await ApplicationData.Current.TemporaryFolder.TryGetItemAsync("e3log.json");
                if (e3log != null)
                {
                    await e3log.DeleteAsync();
                }

                await FullTrustProcessLauncher.LaunchFullTrustProcessForCurrentAppAsync("ECBackground");
                await Task.Delay(10000);

                e3log = await ApplicationData.Current.TemporaryFolder.TryGetItemAsync("e3log.json");
                if (e3log != null)
                {
                    var text = await FileIO.ReadTextAsync((IStorageFile)e3log);
                    var logs = JsonConvert.DeserializeObject<PowerLog[]>(text);

                    var networkRepository = new ElectricityNetworkRepository();
                    var configurationRepository = new MockupConfigRepository();
                    var deviceConsumptionRepository = new DeviceConsumptionRepository(networkRepository, configurationRepository);


                    var consumptions =
                        logs.Select(l => new Consumption
                        {
                            At = l.At,
                            mJOnBattery = l.BatteryPower,
                            mJPluggedIn = l.PluggedInPower,
                        }).ToArray();

                    await deviceConsumptionRepository.ReportDeviceConsumptionAsync(consumptions);
                }
            }
            finally
            {
                deferral.Complete();
            }
        }

        #endregion
    }
}
