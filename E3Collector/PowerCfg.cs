using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;

namespace E3Collector
{
    public class PowerCfg
    {
        #region Methods

        public static async Task CollectBackground(string deviceId)
        {
#if DEBUG
            //Debugger.Launch();
#endif

            var filePath = System.IO.Path.GetTempFileName();
            try
            {
                // Launch srumutil which will append live results to a file on disk

                //collect last hour of data
                await LaunchPowerCFG(filePath);
                var logs = ExtractPowerInfoPer15Mins(filePath);
                var consumptionLog = new ConsumptionLog()
                {
                    DeviceId = deviceId,
                    Values = logs
                };
                await DeviceConsumption.ReportDeviceConsumptionAsync(consumptionLog);
                //File.WriteAllText(Path.Combine(ApplicationData.Current.TemporaryFolder.Path, "e3log.json"), fileContent);
            }
            finally
            {
                File.Delete(filePath);
            }
        }

        private static List<Consumption> ExtractPowerInfoPer15Mins(string filePath)
        {
            // only read data from last hour
            var refDateTime = DateTimeOffset.UtcNow.AddHours(-2).ToString("yyyy-MM-dd:HH:mm:ss.0000");
            var maxDuration = TimeSpan.FromMinutes(15).TotalMilliseconds;

            using (var fileStream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
            using (var reader = new StreamReader(fileStream))
            {
                //ignore header
                var line = reader.ReadLine();

                List<Consumption> logs = new List<Consumption>();
                Consumption currentLog = null;

                while ((line = reader.ReadLine()) != null)
                {
                    var lineLog = ReadLogFromPowerCfgLine(line, refDateTime, maxDuration);
                    if (lineLog != null)
                    {
                        var roundedLineLogAt = lineLog.At.RoundToQuarter();
                        if (currentLog == null || roundedLineLogAt != currentLog.At)
                        {
                            currentLog = new Consumption
                            {
                                At = roundedLineLogAt,
                                mJPluggedIn = lineLog.mJPluggedIn,
                                mJOnBattery = lineLog.mJOnBattery
                            };

                            logs.Add(currentLog);
                        }
                        else
                        {
                            currentLog.mJOnBattery += lineLog.mJOnBattery;
                            currentLog.mJPluggedIn += lineLog.mJPluggedIn;
                        }
                    }
                }

                return logs;
            }
        }

        private static Task<int> LaunchPowerCFG(string tempFile)
        {
            var process = "powercfg.exe";
            var processArgs = $"/SRUMUTIL /output \"{tempFile}\"";
            return Process.LaunchProcess(process, processArgs, "runas", CancellationToken.None);
        }

        private static string ReadColumn(int[] separatorIndexes, int fieldIndex, string line)
        {
            if (fieldIndex >= separatorIndexes.Length)
            {
                return line.Substring(separatorIndexes[separatorIndexes.Length - 1] + 2);
            }

            var startIndex = 0;
            var endIndex = separatorIndexes[fieldIndex];
            if (fieldIndex != 0)
            {
                startIndex = separatorIndexes[fieldIndex - 1] + 2;
            }
            return line.Substring(startIndex, endIndex - startIndex);
        }

        //powercfg: AppId	 UserId	 TimeStamp	 OnBattery	 ScreenOn	 BatterySaverActive	 LowPowerEpochActive	 Foreground	 InteractivityState	 Container	 Committed	 TimeInMSec	 MeasuredBitmap	 EnergyLoss	 CPUEnergyConsumption	 SocEnergyConsumption	 DisplayEnergyConsumption	 DiskEnergyConsumption	 NetworkEnergyConsumption	 MBBEnergyConsumption	 OtherEnergyConsumption	 EmiEnergyConsumption	 CPUEnergyConsumptionWorkOnBehalf	 CPUEnergyConsumptionAttributed	 TotalEnergyConsumption
        //srumutil: AppId	 UserId	 TimeStamp	 MeasuredPower	 OnBattery	 Foreground	 ScreenOn	 BatterySaverActive	 LowPowerEpochActive	 InteractivityState	 Committed	 TimeInMSec	 MeasuredBitmap	 EnergyLoss	 CPUEnergyConsumption	 SocEnergyConsumption	 DisplayEnergyConsumption	 DiskEnergyConsumption	 NetworkEnergyConsumption	 MBBEnergyConsumption	 OtherEnergyConsumption	 EmiEnergyConsumption	 TotalEnergyConsumption

        private static Consumption ReadLogFromPowerCfgLine(string line, string refDateTime, double maxDuration)
        {
            var separatorIndexes = new int[24];
            var currentSeparatorIndex = 0;
            for (int i = 0; i < line.Length; i++)
            {
                if (line[i] == ',')
                {
                    separatorIndexes[currentSeparatorIndex++] = i;
                }
            }

            string rawTimestamp = ReadColumn(separatorIndexes, 2, line);
            if (String.Compare(rawTimestamp, refDateTime) >= 0)
            {
                var power = ReadColumn(separatorIndexes, 24, line);
                var durationString = ReadColumn(separatorIndexes, 11, line);
                if (long.TryParse(power, out var p) && long.TryParse(durationString, out var duration) && duration < maxDuration)
                {
                    var onBattery = ReadColumn(separatorIndexes, 3, line) == "TRUE";
                    var sbAt = new StringBuilder(rawTimestamp);
                    sbAt[10] = 'T';
                    var at = DateTimeOffset.Parse(sbAt.ToString());
                    if (onBattery)
                    {
                        return new Consumption
                        {
                            At = at,
                            mJOnBattery = p
                        };
                    }
                    else
                    {
                        return new Consumption
                        {
                            At = at,
                            mJPluggedIn = p
                        };
                    }
                }
            }
            return null;
        }

        #endregion Methods
    }
}