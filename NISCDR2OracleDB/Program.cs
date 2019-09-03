using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NISCDR2OracleDB.Contracts;
using System;
using System.IO;


namespace NISCDR2OracleDB
{
    public class Program
    {
        public static IConfigurationRoot Configuration { get; set; }

        static void Main()
        {
            try
            {
                //Environment.SetEnvironmentVariable("ASPNETCORE_SecretStuff__OracleUserId", "tra", EnvironmentVariableTarget.User);

                var envVariable = Environment.GetEnvironmentVariable("ASPNETCORE_NISCDR2OracleDb_ENVIRONMENT"); //using ASPNETCORE_NISCDR2OracleDb_ as a prefix in order to be able to distinguish the config (environment variables) specific for this project 
                var isDevelopment = string.IsNullOrEmpty(envVariable) || envVariable.ToLower() == "development";
                //Determines the working environment as IHostingEnvironment is unavailable in a console app

                //https://cpratt.co/asp-net-core-configuration-with-environment-variables-in-iis/
                //above link explains how to use UserSecrets while in Dev and Environmet variables while in Production (IIS)
                var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    //.AddJsonFile($"appsettings.{envVariable}.json", optional: true, reloadOnChange: true) //not needed, used Environment variables instead
                    //If adding new Environment variables in order to take effect VS must be restarted
                    .AddEnvironmentVariables(prefix: "ASPNETCORE_NISCDR2OracleDb_");

                //only add secrets in development, else use Environment variables. Wonder why?!, 
                //check this: https://stackoverflow.com/questions/39668456/how-to-deploy-asp-net-core-usersecrets-to-production
                //shortly:  Secrets are just to keep sensitive data from being committed within the code to the repo. 
                //In production set the secrets values to the appsettings or env variables or any other config source.

                if (isDevelopment)
                { //if Development the Environment Variables will be overwritten with the User Secrets
                    builder.AddUserSecrets<SecretStuff>();
                    //If the keys of the secrets are equal to the keys in the previously defined appsettings.json, the app settings will be overwritten.
                }

                Configuration = builder.Build();


                //setup our DI
                var serviceProvider = new ServiceCollection()
                    .Configure<SecretStuff>(Configuration.GetSection(nameof(SecretStuff)))
                    .AddOptions()
                    .AddSingleton<ICDR, CDR>()
                    .AddSingleton<IEngine, Engine>()
                    .AddSingleton<IService, Service>()

                    .BuildServiceProvider();

                var engine = serviceProvider.GetService<IEngine>();
                engine.Run();
            }

            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine(DateTime.Now);
                Console.WriteLine("Error : {0}", ex);
                Console.WriteLine();

                //Log error to Windows EventViewer in a future app release :)
                //Logger.Log(ex);
            }
        }
    }
}
