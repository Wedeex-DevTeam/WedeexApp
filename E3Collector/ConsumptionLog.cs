namespace E3Collector
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [DataContract]
    public class ConsumptionLog
    {
        #region Properties

        [DataMember(Name = "deviceId")]
        public string DeviceId { get; set; }

        [DataMember(Name = "values")]
        public List<Consumption> Values { get; set; }

        #endregion Properties
    }
}