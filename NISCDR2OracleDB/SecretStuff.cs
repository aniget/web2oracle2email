namespace NISCDR2OracleDB
{
    public class SecretStuff
    {
        public string OracleConnectionString { get; set; }
        public string CDRFileDownloadUrl { get; set; }
        public string ConnectionProxyName { get; set; }
        public string ConnectionProxyPort { get; set; }
        public string DailyExportScript { get; set; }
        public string DailyExportFilePath { get; set; }
        public string DailyExportFileName { get; set; }
        public string EmailFrom { get; set; }
        public string EmailTo { get; set; }
        public string EmailCc { get; set; }
        public string EmailSubject { get; set; }
        public string EmailBody { get; set; }
        public string EmailHost { get; set; }

    }

}
