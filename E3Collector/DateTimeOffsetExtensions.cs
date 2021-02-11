using System;

namespace E3Collector
{
    public static class DateTimeOffsetExtensions
    {
        #region Methods

        public static DateTimeOffset RoundToQuarter(this DateTimeOffset dt)
        {
            dt = dt.ToUniversalTime();
            return new DateTimeOffset(dt.Year, dt.Month, dt.Day, dt.Hour, RoundMinuteToQuarter(dt.Minute), 0, 0, TimeSpan.Zero);
        }

        private static int RoundMinuteToQuarter(int minute)
        {
            if (minute < 15) return 0;
            if (minute < 30) return 15;
            if (minute < 45) return 30;
            return 45;
        }

        #endregion Methods
    }
}