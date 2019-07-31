using Microsoft.Extensions.Options;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;

namespace NISCDR2OracleDB
{
    public class CDR : ICDR

    {
        private readonly SecretStuff secrets;

        public CDR(IOptions<SecretStuff> secrets)
        {
            this.secrets = secrets.Value;
        }

        public void Download(string downloadUrl, string csvFile, string connectioProxyName, string connectionProxyPort)
        {
            try
            {

                using (var client = new WebClient())
                {
                    WebProxy wp = new WebProxy(secrets.ConnectionProxyName.ToString(), int.Parse(secrets.ConnectionProxyPort));
                    client.Proxy = wp;

                    //validate whether cdrFile is generated the same day as the day of the download
                    client.OpenRead(downloadUrl);
                    DateTime lastWriteDate = Convert.ToDateTime(client.ResponseHeaders["Last-Modified"]);
                    if (lastWriteDate.Date != DateTime.Now.Date)
                    {
                        throw new ArgumentException("CDR file is not with today's date");
                    }

                    client.DownloadFile(downloadUrl, csvFile);

                }

            }
            catch
            {
                throw;
            }



        }

        public List<CDRModel> ReadCSV2List(string csvFile, bool readOnlyFirstCDR = false)
        {
            try
            {
                int numberOfLinesInCDRsFileToBeRead;

                if (readOnlyFirstCDR)
                {
                    numberOfLinesInCDRsFileToBeRead = 1;
                }
                else
                {
                    numberOfLinesInCDRsFileToBeRead = File.ReadLines(csvFile).Count();
                }

                var cdrData = File.ReadAllLines(csvFile).Skip(1).Take(numberOfLinesInCDRsFileToBeRead).Select(columns => columns.Split(',')).Select(columns => new CDRModel()
                {
                    //StartTime = DateTime.ParseExact((columns[0].Substring(1)).Substring(0, (columns[0].Substring(1)).Length - 1), dateTimeFormat, CultureInfo.InvariantCulture),
                    StartTime = columns[0].Substring(1).Substring(0, (columns[0].Substring(1)).Length - 1),
                    Anumber = columns[1],
                    Bnumber = columns[2],
                    Cnumber = columns[3],
                    DestPrefix = long.Parse(columns[4]),
                    BuyRate = decimal.Parse(columns[5]),
                    SellRate = decimal.Parse(columns[6]),
                    Duration = ushort.Parse(columns[7]),
                    PartnerId = ushort.Parse(columns[8]),
                    Trunk = columns[9],
                    TerminateCauseId = byte.Parse(columns[10]),
                    Sipiax = byte.Parse(columns[11]),
                    BuyAmount = decimal.Parse(columns[12]),
                    SellAmount = Math.Round(decimal.Parse(columns[6]) * ushort.Parse(columns[7]) / 60, 6),
                    Margin = decimal.Parse(columns[14]),
                    Markup = decimal.Parse(columns[15]),
                    CallId = decimal.Parse(columns[16])
                }).ToList();

                return cdrData;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void Load2OracleDb(List<CDRModel> cdrList, string connectionString)
        {
            //https://www.c-sharpcorner.com/article/two-ways-to-insert-bulk-data-into-oracle-database-using-c-sharp/
            //https://www.oracle.com/webfolder/technetwork/tutorials/obe/db/dotnet/ODPNET_Core_get_started/index.html
            //https://blogs.oracle.com/oraclemagazine/put-your-arrays-in-a-bind
            //http://www.gasnet.com.br/upmr/upusers/odpnet.pdf


            using (OracleConnection con = new OracleConnection(connectionString))
            {
                using (OracleCommand cmd = con.CreateCommand())
                {
                    try
                    {
                        con.Open();
                        //cmd.BindByName = true; //only uncomment if you want to bind parameters by their name

                        //create command parameters
                        OracleParameter startTime = new OracleParameter(); startTime.OracleDbType = OracleDbType.Varchar2;
                        OracleParameter aNumber = new OracleParameter(); aNumber.OracleDbType = OracleDbType.Varchar2;
                        OracleParameter bNumber = new OracleParameter(); bNumber.OracleDbType = OracleDbType.Varchar2;
                        OracleParameter cNumber = new OracleParameter(); cNumber.OracleDbType = OracleDbType.Varchar2;
                        OracleParameter destPrefix = new OracleParameter(); destPrefix.OracleDbType = OracleDbType.Decimal;
                        OracleParameter buyRate = new OracleParameter(); buyRate.OracleDbType = OracleDbType.Decimal;
                        OracleParameter sellRate = new OracleParameter(); sellRate.OracleDbType = OracleDbType.Decimal;
                        OracleParameter duration = new OracleParameter(); duration.OracleDbType = OracleDbType.Int16;
                        OracleParameter partnerId = new OracleParameter(); partnerId.OracleDbType = OracleDbType.Int32;
                        OracleParameter trunk = new OracleParameter(); trunk.OracleDbType = OracleDbType.Varchar2;
                        OracleParameter terminateCauseId = new OracleParameter(); terminateCauseId.OracleDbType = OracleDbType.Int16;
                        OracleParameter sipiax = new OracleParameter(); sipiax.OracleDbType = OracleDbType.Int16;
                        OracleParameter buyAmount = new OracleParameter(); buyAmount.OracleDbType = OracleDbType.Decimal;
                        OracleParameter sellAmount = new OracleParameter(); sellAmount.OracleDbType = OracleDbType.Decimal;
                        OracleParameter margin = new OracleParameter(); margin.OracleDbType = OracleDbType.Decimal;
                        OracleParameter markup = new OracleParameter(); markup.OracleDbType = OracleDbType.Decimal;
                        OracleParameter callId = new OracleParameter(); callId.OracleDbType = OracleDbType.Decimal;


                        //add parameters to collection
                        cmd.Parameters.Add(startTime);
                        cmd.Parameters.Add(aNumber);
                        cmd.Parameters.Add(bNumber);
                        cmd.Parameters.Add(cNumber);
                        cmd.Parameters.Add(destPrefix);
                        cmd.Parameters.Add(buyRate);
                        cmd.Parameters.Add(sellRate);
                        cmd.Parameters.Add(duration);
                        cmd.Parameters.Add(partnerId);
                        cmd.Parameters.Add(trunk);
                        cmd.Parameters.Add(terminateCauseId);
                        cmd.Parameters.Add(sipiax);
                        cmd.Parameters.Add(buyAmount);
                        cmd.Parameters.Add(sellAmount);
                        cmd.Parameters.Add(margin);
                        cmd.Parameters.Add(markup);
                        cmd.Parameters.Add(callId);

                        var numberOfCDRs = cdrList.Count;
                        //var numberOfCDRs = 40000;

                        //instantiate arrays to hold the values of the parameters
                        string[] cdrStartTimes = new string[numberOfCDRs];
                        string[] cdrAnumbers = new string[numberOfCDRs];
                        string[] cdrBnumbers = new string[numberOfCDRs];
                        string[] cdrCnumbers = new string[numberOfCDRs];
                        Decimal[] cdrDestPrefixes = new Decimal[numberOfCDRs];
                        Decimal[] cdrBuyRates = new Decimal[numberOfCDRs];
                        Decimal[] cdrSellRates = new Decimal[numberOfCDRs];
                        int[] cdrDurations = new int[numberOfCDRs];
                        int[] cdrPartnerIds = new int[numberOfCDRs];
                        string[] cdrTrunks = new string[numberOfCDRs];
                        int[] cdrTerminateCauseIds = new int[numberOfCDRs];
                        int[] cdrSipiaxes = new int[numberOfCDRs];
                        Decimal[] cdrBuyAmounts = new Decimal[numberOfCDRs];
                        Decimal[] cdrSellAmounts = new Decimal[numberOfCDRs];
                        Decimal[] cdrMargins = new Decimal[numberOfCDRs];
                        Decimal[] cdrMarkups = new Decimal[numberOfCDRs];
                        Decimal[] cdrCallIds = new Decimal[numberOfCDRs];


                        DataTable dt = new DataTable();
                        dt.Columns.Add("cdrStartTime");
                        dt.Columns.Add("cdrAnumber");
                        dt.Columns.Add("cdrBnumber");
                        dt.Columns.Add("cdrCnumber");
                        dt.Columns.Add("cdrDestPrefix");
                        dt.Columns.Add("cdrBuyRate");
                        dt.Columns.Add("cdrSellRate");
                        dt.Columns.Add("cdrDuration");
                        dt.Columns.Add("cdrPartnerId");
                        dt.Columns.Add("cdrTrunk");
                        dt.Columns.Add("cdrTerminateCauseId");
                        dt.Columns.Add("cdrSipiax");
                        dt.Columns.Add("cdrBuyAmount");
                        dt.Columns.Add("cdrSellAmount");
                        dt.Columns.Add("cdrMargin");
                        dt.Columns.Add("cdrMarkup");
                        dt.Columns.Add("cdrCallId");

                        //fill in the data table with info from the CDRs
                        foreach (var cdr in cdrList)
                        {
                            DataRow dr = dt.NewRow();
                            dr["cdrStartTime"] = cdr.StartTime;
                            dr["cdrAnumber"] = cdr.Anumber;
                            dr["cdrBnumber"] = cdr.Bnumber;
                            dr["cdrCnumber"] = cdr.Cnumber;
                            dr["cdrDestPrefix"] = cdr.DestPrefix;
                            dr["cdrBuyRate"] = cdr.BuyRate;
                            dr["cdrSellRate"] = cdr.SellRate;
                            dr["cdrDuration"] = cdr.Duration;
                            dr["cdrPartnerId"] = cdr.PartnerId;
                            dr["cdrTrunk"] = cdr.Trunk;
                            dr["cdrTerminateCauseId"] = cdr.TerminateCauseId;
                            dr["cdrSipiax"] = cdr.Sipiax;
                            dr["cdrBuyAmount"] = cdr.BuyAmount;
                            dr["cdrSellAmount"] = cdr.SellAmount;
                            dr["cdrMargin"] = cdr.Margin;
                            dr["cdrMarkup"] = cdr.Markup;
                            dr["cdrCallId"] = cdr.CallId;

                            dt.Rows.Add(dr);
                        }

                        //convert the info from the data tables to the arrays that will feed the parameters
                        //for (var i = 0; i < dt.Rows.Count; i++)
                        for (var i = 0; i < numberOfCDRs; i++)
                        {
                            cdrStartTimes[i] = Convert.ToString(dt.Rows[i]["cdrStartTime"]);
                            cdrAnumbers[i] = Convert.ToString(dt.Rows[i]["cdrAnumber"]);
                            cdrBnumbers[i] = Convert.ToString(dt.Rows[i]["cdrBnumber"]);
                            cdrCnumbers[i] = Convert.ToString(dt.Rows[i]["cdrCnumber"]);
                            cdrDestPrefixes[i] = Convert.ToDecimal(dt.Rows[i]["cdrDestPrefix"]);
                            cdrBuyRates[i] = Convert.ToDecimal(dt.Rows[i]["cdrBuyRate"]);
                            cdrSellRates[i] = Convert.ToDecimal(dt.Rows[i]["cdrSellRate"]);
                            cdrDurations[i] = Convert.ToInt16(dt.Rows[i]["cdrDuration"]);
                            cdrPartnerIds[i] = Convert.ToInt16(dt.Rows[i]["cdrPartnerId"]);
                            cdrTrunks[i] = Convert.ToString(dt.Rows[i]["cdrTrunk"]);
                            cdrTerminateCauseIds[i] = Convert.ToInt16(dt.Rows[i]["cdrTerminateCauseId"]);
                            cdrSipiaxes[i] = Convert.ToInt16(dt.Rows[i]["cdrSipiax"]);
                            cdrBuyAmounts[i] = Convert.ToDecimal(dt.Rows[i]["cdrBuyAmount"]);
                            cdrSellAmounts[i] = Convert.ToDecimal(dt.Rows[i]["cdrSellAmount"]);
                            cdrMargins[i] = Convert.ToDecimal(dt.Rows[i]["cdrMargin"]);
                            cdrMarkups[i] = Convert.ToDecimal(dt.Rows[i]["cdrMarkup"]);
                            cdrCallIds[i] = Convert.ToDecimal(dt.Rows[i]["cdrCallId"]);
                        };


                        //set values of the command parameters
                        startTime.Value = cdrStartTimes;
                        aNumber.Value = cdrAnumbers;
                        bNumber.Value = cdrBnumbers;
                        cNumber.Value = cdrCnumbers;
                        destPrefix.Value = cdrDestPrefixes;
                        buyRate.Value = cdrBuyRates;
                        sellRate.Value = cdrSellRates;
                        duration.Value = cdrDurations;
                        partnerId.Value = cdrPartnerIds;
                        trunk.Value = cdrTrunks;
                        terminateCauseId.Value = cdrTerminateCauseIds;
                        sipiax.Value = cdrSipiaxes;
                        buyAmount.Value = cdrBuyAmounts;
                        sellAmount.Value = cdrSellAmounts;
                        margin.Value = cdrMargins;
                        markup.Value = cdrMarkups;
                        callId.Value = cdrCallIds;

                        //build the command string
                        //https://stackoverflow.com/questions/12765181/ora-01843-not-a-valid-month-working-on-db-but-not-when-doing-the-same-on-asp
                        cmd.CommandText = "insert into " + Constants.cdrTable + " (StartTime, A_number, B_number, C_number, DestPrefix, BuyRate, SellRate, DURATION, PartnerId, Trunk, TerminateCauseId, Sipiax, BuyAmount, SellAmount, Margin, Markup, CallId) values (TO_DATE(:1,'RRRR-MM-DD HH24:MI:SS'), :2, :3, :4, :5, :6, :7, :8, :9, :10, :11, :12, :13, :14, :15, :16, :17)";

                        //set the number of elements that each array contains
                        cmd.ArrayBindCount = cdrBnumbers.Length;

                        //if there is even one record that is not unique (unique constraint is violated), none of the CDRs are inserted
                        //because this is a Bulk insert
                        cmd.ExecuteNonQuery();

                        cmd.Parameters.Clear();
                        cmd.Dispose();

                        con.Close();
                        con.Dispose();
                    }
                    catch
                    {
                        throw;
                    }
                }
            }

        }

        public void ValidateList(List<CDRModel> cdrList)
        {
            try
            {
                //validates a random CDR whether it is from the preiovus day 
                DateTime sampleCDRDate = DateTime.ParseExact(cdrList[3].StartTime, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                var timeSpan = DateTime.Now.Date - sampleCDRDate;
                if (timeSpan.Days != 0) //CDRs are not from preiovus day 
                {
                    throw new ArgumentException("CDRs are not from previous day");
                }
            }
            catch
            {
                throw;
            }
        }

        public bool DbAlreadyContainsCDRsWithStartDate(DateTime firstCDRDate, string connectionString)
        {
            using (OracleConnection con1 = new OracleConnection(connectionString))
            {
                using (OracleCommand cmd = con1.CreateCommand())
                {
                    try
                    {
                        con1.Open();

                        cmd.CommandText = "select StartTime from " + Constants.cdrTable + " where to_char(trunc(StartTime),'DD.MM.YYYY') = '" + firstCDRDate.ToString("dd.MM.yyyy") + "'";

                        OracleDataReader reader = cmd.ExecuteReader();

                        while (reader.Read())
                        { //if there is something to read then there are already CDRs with the same date in the Database
                            return true;
                        }

                        reader.Close();
                        con1.Close();
                        con1.Dispose();

                    }
                    catch
                    {
                        throw;
                    }
                }
            }

            return false;
        }
    }
}
