using System;

namespace NISCDR2OracleDB
{
    public class ServiceModel : IServiceModel
    {
        public DateTime Service_Date { get; set; }
        public string Service_Name { get; set; }
        public decimal Number_Of_Calls { get; set; }
        public decimal Duration_In_Min { get; set; }
        public decimal Amount_In_Eur { get; set; }
    }
}
