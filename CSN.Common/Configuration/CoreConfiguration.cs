namespace CSN.Common.Configuration
{
    using System.Runtime.Serialization;

    [DataContract]
    public class CoreConfiguration
    {

        [DataMember(Name = "apiKey")]
        public string ApiKey { get; set; }

        [DataMember(Name = "consumptionReportUrl")]
        public string ConsumptionReportUrl { get; set; }

        [DataMember(Name = "consumptionSummaryUrl")]
        public string ConsumptionSummaryUrl { get; set; }

        [DataMember(Name = "notificationHubConnectionString")]
        public string NotificationHubConnectionString { get; set; }

        [DataMember(Name = "notificationHubPath")]
        public string NotificationHubPath { get; set; }

        [DataMember(Name = "osPowerInWatt")]
        public double OsPowerInWatt { get; set; }

        [DataMember(Name = "signatureId")]
        public string SignatureId { get; set; }

        [DataMember(Name = "signatureKey")]
        public string SignatureKey { get; set; }

        [DataMember(Name = "telemetryInstrumentationKey")]
        public string TelemetryInstrumentationKey { get; set; }
    }
}
