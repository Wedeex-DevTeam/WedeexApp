using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;

namespace E3Collector
{
    public class SRUMUtil
    {
        #region Methods

        public static Task CollectRealtime()
        {
            return AppService.ConnectToAppService("EnergyCollectorRealtime", async (connection, cts) =>
            {
                var filePath = System.IO.Path.GetTempFileName();

                // Launch srumutil which will append live results to a file on disk

                var launchedTask = LaunchSRUMUtil(true, 60, filePath, cts.Token);
                await Task.Delay(500);

                // Launch a task reading the lines from the same file
                RealtimeReadFile(connection, filePath, cts.Token).NotAwaited();

                // await srumutil end of execution
                await launchedTask;

                File.Delete(filePath);
            });
        }

        private static Task<int> LaunchSRUMUtil(bool isRealtime, double duration, string tempFile, CancellationToken token)
        {
            var processPath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
            var srumutilPath = Path.Combine(Path.GetDirectoryName(processPath), "srumutil.exe");

            var processArgs = new List<string>();
            processArgs.Add("-s user");
            if (isRealtime)
            {
                processArgs.Add("-rt");
            }
            //if (isUser)
            //{
            //    processArgs.Add("-s user");
            //}
            //else
            //{
            //    processArgs.Add("-s all");
            //    info.Verb = "runas";
            //}
            processArgs.Add("-t " + duration);
            processArgs.Add("-o \"" + tempFile + "\"");

            return Process.LaunchProcess(srumutilPath, String.Join(" ", processArgs), null, token);
        }

        private static async Task RealtimeReadFile(AppServiceConnection connection, string filePath, CancellationToken token)
        {
            try
            {
                var sb = new StringBuilder();

                using (var fileStream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
                using (var reader = new StreamReader(fileStream))
                {
                    while (!token.IsCancellationRequested)
                    {
                        var data = await reader.ReadToEndAsync();
                        if (data.Length > 0)
                        {
                            // append data to buffer
                            sb.Append(data);

                            // process only new complete lines
                            var lastNewLine = -1;
                            for (int i = sb.Length - 1; i >= 0; i--)
                            {
                                if (sb[i] == '\n')
                                {
                                    lastNewLine = i;
                                    break;
                                }
                            }

                            if (lastNewLine != -1)
                            {
                                var lines = sb.ToString(0, lastNewLine);
                                sb.Remove(0, lines.Length);
                                if (!token.IsCancellationRequested)
                                {
                                    // process pack of lines and send to app
                                    SendRealtimeData(connection, lines);
                                }
                            }
                        }
                        await Task.Delay(1000);
                    }
                }
            }
            catch
            {
            }
        }

        private static void SendRealtimeData(AppServiceConnection connection, string rawLines)
        {
            try
            {
                var lines = rawLines.Split('\n');
                // total power is at last column
                var power = 0L;
                var hasPower = false;
                foreach (var line in lines)
                {
                    var idx = line.LastIndexOf(',');
                    //ignore lines not containing digit in totalEnergy
                    if (long.TryParse(line.Substring(idx + 1), out var appPower))
                    {
                        hasPower = true;
                        power += appPower;
                    }
                }

                if (hasPower)
                {
                    var valueSet = new ValueSet();
                    valueSet.Add("realtimePower", power);
                    connection.SendMessageAsync(valueSet).AsTask().NotAwaited();
                }
            }
            catch
            {
            }
        }

        #endregion Methods
    }
}