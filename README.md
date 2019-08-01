WEB2ORACLE
=============

Description:
This is ASPNET Core 2.0 Console Application that downloads data file in predefined (csv) format

The application follows these steps:
1. Download CSV File From Web Server on a daily basis (scheduled in Task Scheduler or similar)
2. Read CSV file into a collection
3. Validate information in CSV file itself
4. Validate information in CSV file prior to Db Insert against the information in the Db
5. Build the Command text by using multiple command parameters of different types
6. Bulk Insert the information into a table in Oracle Database using Oracle ManagedDataAccess Driver

Console application contains:
- DI using constructor injections and the native .Net Core DI container to instantiate types, 
- Interfaces where applicable, 
- User Secrets on Development machine to keep the connection string, URL for web file download, etc.
- Same information is stored as Environment variables on Production machine
- Logger class is ready to go but is not switched into the configuration as there is no special need at the moment
- The console application is deployed (self-contained deployment) to a server


