using Microsoft.Extensions.Options;
using NISCDR2OracleDB.Contracts;
using System;

namespace NISCDR2OracleDB
{
    public class Engine : IEngine
    {
        private readonly ICDR cdrFile;
        private readonly SecretStuff secrets;
        private readonly IService service;

        public Engine(ICDR cdrFile, IOptions<SecretStuff> secrets, IService service)
        {
            this.cdrFile = cdrFile ?? throw new ArgumentNullException(nameof(cdrFile));
            this.secrets = secrets.Value ?? throw new ArgumentNullException(nameof(secrets));
            this.service = service ?? throw new ArgumentNullException(nameof(service));
        }

        public void Run()
        {
            try
            {
                cdrFile.Download(secrets.CDRFileDownloadUrl, Constants.csvFile, secrets.ConnectionProxyName, secrets.ConnectionProxyPort);

                //verify that there are no CDRs in the DB with the date of the first CDR in the file (to avoid unnecessary attempt for insert)
                var firstCDR = cdrFile.ReadCSV2List(Constants.csvFile, true)[0];
                DateTime firstCDRDate = DateTime.Parse(firstCDR.StartTime).Date;
                if (cdrFile.DbAlreadyContainsCDRsWithStartDate(firstCDRDate, secrets.OracleConnectionString))
                {
                    throw new ArgumentException("there are CDRs in the DB with the date of the first CDR in the file ");
                }

                var cdrList = cdrFile.ReadCSV2List(Constants.csvFile);

                //validate cdrList
                cdrFile.ValidateList(cdrList);

                //Load CDRs to DB
                cdrFile.Load2OracleDb(cdrList, secrets.OracleConnectionString);


                //Get services data: Get Daily figures from Oracle DB and load them in a collection
                var dataFromDb = service.GetData(secrets.OracleConnectionString, secrets.DailyExportScript);


                //Save the collected data into Excel file in xlsx format
                service.ExportDataToExcelUsingEPPlus(dataFromDb, secrets.DailyExportFilePath, secrets.DailyExportFileName);


                //Send email with the attached Excel file using Windows authentication
                service.SendExportedDataOverEmail(secrets.EmailFrom, secrets.EmailTo, secrets.EmailCc, secrets.EmailSubject, secrets.EmailBody, secrets.EmailHost, secrets.DailyExportFilePath + secrets.DailyExportFileName);

            }

            catch
            {
                throw;
            }

        }
    }
}
