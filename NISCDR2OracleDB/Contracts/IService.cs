using System.Collections.Generic;

namespace NISCDR2OracleDB.Contracts
{
    public interface IService
    {
        ICollection<IServiceModel> GetData(string connectionString, string dailyExportScript);
        void ExportDataToExcelUsingEPPlus(ICollection<IServiceModel> dataFromDb, string filePath, string fileName);
        void SendEmail(string emailFrom, string EmailTo, string emailSubject, string emailBody, string EmailHost, string emailCc
            = "", bool attachmentIsRequired = false, string filePathAndName = "");
    }
}
