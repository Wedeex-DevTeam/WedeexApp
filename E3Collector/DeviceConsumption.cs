namespace E3Collector
{
    using CSN.Common.Core;
    using Newtonsoft.Json;
    using System.Net.Http;
    using System.Threading.Tasks;

    public class DeviceConsumption
    {
        #region Fields

        private static HttpClient httpClient = new HttpClient(new SignatureDelegateHandler());

        #endregion Fields

        #region Methods

        internal static async Task ReportDeviceConsumptionAsync(ConsumptionLog consumptionLog)
        {
            var json = JsonConvert.SerializeObject(consumptionLog);

            if (CoreConfigurationManager.Instance.Configuration == null)
            {
                await CoreConfigurationManager.Instance.InitAsync();
            }

            var result = await httpClient.PostAsync(CoreConfigurationManager.Instance.Configuration.ConsumptionReportUrl,
                new StringContent(json, System.Text.Encoding.UTF8, "application/json"));

            result.EnsureSuccessStatusCode();
        }

        #endregion Methods
    }
}