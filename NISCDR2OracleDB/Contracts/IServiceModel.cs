using System;

namespace NISCDR2OracleDB
{
    public interface IServiceModel
    {
        DateTime Service_Date { get; set; }
        string Service_Name { get; set; }
        decimal Number_Of_Calls { get; set; }
        decimal Duration_In_Min { get; set; }
        decimal Amount_In_Eur { get; set; }

    }
}