using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.AppService;

namespace E3Collector
{
    public static class AppService
    {
        #region Methods

        public static async Task ConnectToAppService(string name, Func<AppServiceConnection, CancellationTokenSource, Task> exec)
        {
            using (var connection = new AppServiceConnection())
            {
                connection.AppServiceName = name;
                connection.PackageFamilyName = Package.Current.Id.FamilyName;

                AppServiceConnectionStatus status = await connection.OpenAsync();

                if (status != AppServiceConnectionStatus.Success)
                {
                    // Can't connect to AppService for realtime update
                    throw new Exception("Can't connect to app service");
                }
                using (var cts = new CancellationTokenSource())
                {
                    connection.ServiceClosed += delegate { cts.Dispose(); };

                    await exec(connection, cts);
                }
            }
        }

        #endregion Methods
    }
}