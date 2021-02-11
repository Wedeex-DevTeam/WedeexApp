namespace E3Collector
{
    using System;
    using System.Runtime.Serialization;

    [DataContract]
    public class Consumption
    {
        #region Properties

        [DataMember(Name = "at")]
        public DateTimeOffset At { get; set; }

        [DataMember(Name = "mjOnBattery")]
        public double mJOnBattery { get; set; }

        [DataMember(Name = "mjPluggedIn")]
        public double mJPluggedIn { get; set; }

        #endregion Properties
    }
}