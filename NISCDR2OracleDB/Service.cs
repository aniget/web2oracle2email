using OfficeOpenXml;
using OfficeOpenXml.Style;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net.Mail;

namespace NISCDR2OracleDB.Contracts
{
    public class Service : IService
    {
        IEnumerable<IDataRecord> GetFromReader(IDataReader reader)
        {
            while (reader.Read()) yield return reader;
        }

        public ICollection<IServiceModel> GetData(string connectionString, string dailyExportScript)
        {
            using (OracleConnection con = new OracleConnection(connectionString))
            {
                using (OracleCommand cmd = con.CreateCommand())
                {
                    try
                    {
                        con.Open();

                        cmd.CommandText = dailyExportScript;

                        OracleDataReader reader = cmd.ExecuteReader();

                        var serviceData = new List<IServiceModel>();

                        foreach (IDataRecord record in GetFromReader(reader))
                        {
                            IServiceModel serviceModel = new ServiceModel
                            {
                                Year_Month = record.GetString(0),
                                Service_Date = record.GetDateTime(1),
                                Partner = record.GetString(2),
                                Service_Name = record.GetString(3),
                                Number_Of_Calls = record.GetDecimal(4),
                                Duration_In_Min = record.GetDecimal(5),
                                Amount_In_Eur = record.GetDecimal(6)
                            };

                            serviceData.Add(serviceModel);

                        }



                        reader.Close();
                        cmd.Dispose();
                        con.Close();
                        con.Dispose();

                        return serviceData;
                    }
                    catch
                    {
                        throw;
                    }
                }

            }

        }

        public void ExportDataToExcelUsingEPPlus(ICollection<IServiceModel> dataFromDb, string filePath, string fileName)
        {
            try
            {
                using (ExcelPackage excel = new ExcelPackage())
                {
                    var workSheet = excel.Workbook.Worksheets.Add("Sheet1");
                    workSheet.Cells[1, 1].LoadFromCollection(dataFromDb, true);

                    using (ExcelRange dataRange = workSheet.Cells["A1:G10"])
                    {
                        dataRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        dataRange.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        dataRange.Style.Numberformat.Format = "#,##0";
                    }
                    using (ExcelRange dateRange = workSheet.Cells["A2:A9"])
                    {
                        dateRange.Style.Numberformat.Format = "DD.MM.YYYY";
                    }
                    using (ExcelRange sumRange = workSheet.Cells["F10"])
                    {
                        sumRange.Formula = "=SUM(" + workSheet.Cells[2, 6].Address + ":" + workSheet.Cells[9, 6].Address + ")";
                        sumRange.Style.Font.Bold = true;
                    }



                    FileStream objFileStrm = File.Create(fileName);

                    using (ExcelRange dataRangeFilled = workSheet.Cells["A:G"])
                    {
                        dataRangeFilled.AutoFitColumns();
                    }

                    objFileStrm.Close();
                    File.WriteAllBytes(filePath + fileName, excel.GetAsByteArray());

                }

            }
            catch (Exception)
            {
                throw;
            }


        }

        public void SendEmail(string emailFrom, string emailTo, string emailSubject, string emailBody, string emailHost, string emailCc, bool attachmentIsRequired = false, string filePathAndName = "")
        {
            try
            {

                SmtpClient mySmtpClient = new SmtpClient(emailHost);

                // set smtp-client with Windows authentication vs. basicAuthentication
                mySmtpClient.UseDefaultCredentials = true;

                MailMessage myMail = new MailMessage(emailFrom, emailTo, emailSubject, emailBody);

                // set encoding
                if (emailCc != "")
                    myMail.CC.Add(emailCc);

                // set encoding
                myMail.SubjectEncoding = System.Text.Encoding.UTF8;
                myMail.BodyEncoding = System.Text.Encoding.UTF8;

                // text or html
                myMail.IsBodyHtml = true;

                // check for the need of an attachment and send email if such is present
                if (attachmentIsRequired)
                {
                    Attachment attachment = new Attachment(filePathAndName);
                    myMail.Attachments.Add(new Attachment(filePathAndName));

                    if (attachment == null)
                        throw new ArgumentException("No file attached to email and no email sent. Details: There is no such file on the location specified.");
                }

                mySmtpClient.Send(myMail);


            }

            catch (SmtpException ex)
            {
                throw new ApplicationException
                  ("SmtpException has occured: " + ex.Message);
            }
        }
    }
}
