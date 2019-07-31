using Microsoft.Extensions.Options;
using System;


namespace NISCDR2OracleDB
{
    public class Engine : IEngine
    {
        private readonly ICDR cdrFile;
        private readonly SecretStuff secrets;

        public Engine(ICDR cdrFile, IOptions<SecretStuff> secrets)
        {
            this.cdrFile = cdrFile ?? throw new ArgumentNullException(nameof(cdrFile));
            this.secrets = secrets.Value ?? throw new ArgumentNullException(nameof(secrets));
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
            }

            catch
            {
                throw;
            }

        }
    }
}
