using System.Collections.Generic;

namespace NISCDR2OracleDB.Contracts
{
    public interface IService
    {
        ICollection<IServiceModel> GetData(string connectionString, string dailyExportScript);
        void ExportDataToExcelUsingEPPlus(ICollection<IServiceModel> dataFromDb, string filePath, string fileName);
        void SendExportedDataOverEmail(string emailFrom, string EmailTo, string emailCs, string emailSubject, string emailBody, string EmailHost, string filePathAndName);
    }
}
