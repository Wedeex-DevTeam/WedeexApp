using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSN.Uwp.BackgroundTask
{
    public sealed class PowerLog
    {
        public DateTimeOffset At { get; set; }
        public long BatteryPower { get; set; }
        public long PluggedInPower { get; set; }
    }
}
