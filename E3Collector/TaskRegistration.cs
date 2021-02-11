namespace E3Collector
{
    using CSN.Common.Core;
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using System.Xml.Linq;
    using Windows.ApplicationModel.AppService;
    using Windows.Foundation.Collections;

    public class TaskRegistration
    {
        #region Fields

        private static readonly string taskPath = "WeDeex\\EnergyCollector";

        #endregion

        #region Methods

        public static Task CheckRegistration()
        {
            return AppService.ConnectToAppService("EnergyCollectorCheckRegistration", async (connection, cts) =>
            {
                var result = await Process.LaunchProcess("schtasks", $"/Query /TN \"{taskPath}\"", null, cts.Token);

                var valueSet = new ValueSet();
                valueSet.Add("isRegistered", result == 0);

                await connection.SendMessageAsync(valueSet);
            });
        }

        public static Task RegisterTask()
        {
            return AppService.ConnectToAppService("EnergyCollectorRegisterTask", async (connection, cts) =>
            {
                var inputEvent =
                    await Events.FirstEventOf<AppServiceConnection, AppServiceRequestReceivedEventArgs>(
                       cts.Token,
                       h => connection.RequestReceived += h,
                       h => connection.RequestReceived -= h,
                       3000);

                var msg = inputEvent.Request.Message;

                if (msg.TryGetValue("unregister", out var _))
                {
                    await Process.LaunchProcess("schtasks", $"/Delete /F /TN \"{taskPath}\"", "runas", cts.Token);
                    Windows.Storage.ApplicationData.Current.LocalSettings.Values["scheduledTaskRegistered"] = false;
                }
                else if (msg.TryGetValue("deviceId", out var deviceId))
                {
                    if (!Windows.Storage.ApplicationData.Current.LocalSettings.Values.Keys.Contains("scheduledTaskRegistered"))
                    {
                        Windows.Storage.ApplicationData.Current.LocalSettings.Values.Add("scheduledTaskRegistered", false);
                    }

                    try
                    {
                        var xmlTemplateFilePath = CreateScheduledTaskTemplate("background " + deviceId);
                        var registrationCmd = $"/C \"schtasks /Delete /F /TN \"{taskPath}\" & schtasks /create /ru SYSTEM /XML \"{xmlTemplateFilePath}\" /TN \"{taskPath}\"\"";

                        var result = await Process.LaunchProcess("cmd.exe", registrationCmd, "runas", cts.Token);
                        var valueSet = new ValueSet();
                        valueSet.Add("registerTaskResult", result >= 0);
                        await connection.SendMessageAsync(valueSet);
                    }
                    catch
                    {
                        var valueSet = new ValueSet();
                        valueSet.Add("registerTaskResult", false);
                        await connection.SendMessageAsync(valueSet);
                    }
                }
            });
        }

        private static string CreateScheduledTaskTemplate(string arguments)
        {
            var stream = typeof(TaskRegistration).Assembly.GetManifestResourceStream("E3Collector.EnergyCollector.xml");
            var doc = XDocument.Load(stream);

            var file = Path.GetTempFileName();

            var processPath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;

            var execElement =
                doc.Root.Element(XName.Get("Actions", "http://schemas.microsoft.com/windows/2004/02/mit/task"))
                    .Element(XName.Get("Exec", "http://schemas.microsoft.com/windows/2004/02/mit/task"));

            var commandElement = new XElement(XName.Get("Command", "http://schemas.microsoft.com/windows/2004/02/mit/task"), processPath);
            var argumentsElement = new XElement(XName.Get("Arguments", "http://schemas.microsoft.com/windows/2004/02/mit/task"), arguments);

            execElement.Add(commandElement);
            execElement.Add(argumentsElement);

            doc.Save(file);
            return file;
        }

        #endregion Methods
    }
}