using System;
using System.Collections.Generic;

namespace NISCDR2OracleDB
{
    public interface ICDR
    {
        bool DbAlreadyContainsCDRsWithStartDate(DateTime firstCDRDate, string connectionString);
        void Download(string downloadUrl, string csvFile, string connectioProxyName, string connectionProxyPort);
        void Load2OracleDb(List<CDRModel> cdrList, string connectionString);
        List<CDRModel> ReadCSV2List(string csvFile, bool readOnlyFirstCDR = false);
        void ValidateList(List<CDRModel> cdrList);
    }
}