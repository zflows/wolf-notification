using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.WindowsServices;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.IO;
using Wolf.Notification.Database.Entities;

namespace Wolf.Notification
{
    public class Program
    {
        public static int Main(string[] args)
        {
            IHost host;
            try
            {
                Console.WriteLine($"Notification.Service Service starting in {AppContext.BaseDirectory}");
                host = CreateHostBuilder(args).Build();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Log.Fatal(ex, "Host terminated unexpectedly when creating HostBuilder");
                return 1;
            }

            try
            {
                using var scope = host.Services.CreateScope();
                var services = scope.ServiceProvider;
                var context = services.GetRequiredService<NotifDbContext>();
                context.Database.EnsureCreated();
            }
            catch (SqlException ex)
            {
                Console.WriteLine(ex);
                var logger = host?.Services?.GetRequiredService<ILogger<Program>>();
                if (logger != null)
                {
                    logger.LogError(ex, "An error occurred while seeding the database.");
                }
                else
                {
                    Log.Error(ex, "An error occurred while seeding the database.");
                } 
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Log.Fatal(ex, "Host terminated unexpectedly when ensuring DB.");
                return 1;
            }
            

            try
            {
                host.Run();
                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host.Run terminated unexpectedly");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }


        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseWindowsService()
                .UseSerilog()
                .ConfigureAppConfiguration((hostBuilderContext, configurationBinder) =>
                {
                    configurationBinder.AddEnvironmentVariables();

                    bool isWinSvc = WindowsServiceHelpers.IsWindowsService();
                    Console.WriteLine($"\t Current Directory:  {Directory.GetCurrentDirectory()};\r\n\t Windows Service: {isWinSvc};\r\n\t AppContext.BaseDirectory: {AppContext.BaseDirectory};\r\n\t Env: {hostBuilderContext.HostingEnvironment.EnvironmentName}\r\n");
                    Directory.SetCurrentDirectory(AppContext.BaseDirectory); //this is needed for correct relative path (for log configuration for example)
                    configurationBinder.SetBasePath(AppContext.BaseDirectory); //this is also needed to read the appsettings.config from the proper place (even when SetCurrentDirectory was called already)
                    //Serilog.Debugging.SelfLog.Enable(Console.Error);
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
