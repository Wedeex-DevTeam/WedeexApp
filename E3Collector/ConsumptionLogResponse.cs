﻿namespace E3Collector
{
    using System;
    using System.Runtime.Serialization;

    [DataContract]
    public class ConsumptionLogResponse
    {
        #region Properties

        [DataMember(Name = "co2Rate")]
        public double CO2Rate { get; set; }

        [DataMember(Name = "co2RateAverage")]
        public double CO2RateAverage { get; set; }

        [DataMember(Name = "globalSaving")]
        public double GlobalSaving { get; set; }

        [DataMember(Name = "indicator")]
        public string Indicator { get; set; }

        [DataMember(Name = "saving")]
        public double Saving { get; set; }

        [DataMember(Name = "savingDate")]
        public DateTimeOffset SavingDate { get; set; }

        #endregion Properties
    }
}