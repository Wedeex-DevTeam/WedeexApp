namespace CSN.Common.Models
{
    using System.Runtime.Serialization;

    [DataContract]
    public class DeviceConsumption
    {
        #region Properties

        [DataMember(Name = "batteryChargeConsumptionInWatt")]
        public double BatteryChargeConsumptionInWatt { get; set; }

        // Impact in CO2 of the charge of battery has been removed from dashboard but can come back in future
        //[DataMember(Name = "batteryChargeImpactInCO2")]
        //public double BatteryChargeImpactInCO2 { get; set; }

        [DataMember(Name = "deviceConsumptionInWatt")]
        public double DeviceConsumptionInWatt { get; set; }

        [DataMember(Name = "userAppsConsumptionInWatt")]
        public double UserAppsConsumptionInWatt { get; set; }

        [DataMember(Name = "deviceImpactInCO2")]
        public double DeviceImpactInCO2 { get; set; }

        [DataMember(Name = "userCarbonEffortInCO2g")]
        public int UserCarbonEffortInCO2g { get; set; }

        #endregion
    }
}