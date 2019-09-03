WEB2ORACLE2EMAIL
=============

Description:

This is ASPNET Core 2.0 Console Application that performs the following tasks:
1. Download CSV File From Web Server on a daily basis (scheduled in Task Scheduler or similar)
2. Read CSV file into a collection
3. Validate information in CSV file itself
4. Validate information in CSV file prior to Db Insert against the information currently into the Db
5. Build the SQL Command text by using multiple command parameters of different types and based on that
6. Bulk Insert the information into a table in Oracle Database using Oracle ManagedDataAccess Driver
7. Fires a SQL Query to get specific data - daily figures from Oracle DB and load them into a collection
8. Save the collected data into Excel file in "xlsx" format using EPPlus library
9. Send email with the attached Excel file using Windows authentication
10. In case of errors an email will be sent to dedicated recipient with error message


Console application contains:
- DI using constructor injections and the native .Net Core DI container to instantiate types, 
- Interfaces where applicable, 
- User Secrets on Development machine to keep the connection string, URL for web file download, etc.
- Same information is stored as Environment variables on Production machine
- Logger class is ready to go but is not switched into the configuration as there is no special need at the moment
- The console application is deployed (self-contained deployment) to a server


