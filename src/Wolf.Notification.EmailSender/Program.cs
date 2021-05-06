using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.WindowsServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Core;
using Wolf.MessageQueue;
using Wolf.MessageQueue.Services;
using Wolf.Notification.EmailSender.Config;
using Wolf.Notification.EmailSender.OpenAPIs;
using Wolf.Notification.EmailSender.Services;

namespace Wolf.Notification.EmailSender
{
    class Program
    {
        private static void BuildDI(HostBuilderContext context, IServiceCollection services)
        {
            IConfiguration config = context.Configuration;

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(config)
                .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
                .CreateLogger();

            services.Configure<SmtpOptions>(config.GetSection("Smtp"))
                .Configure<QueueOptions>(config.GetSection("QueueOptions"))
                .Configure<NotifApiOptions>(config.GetSection("NotifApi"))
                .AddTransient<Runner>()
                .AddTransient<IMailService, MailService>()
                .AddTransient<IQueueService, RedisMessageQueue>()
                .AddOptions()
                .AddHostedService<Runner>();

            services.AddHttpClient<IMessageService, MessageService>(); //registers service as tansient and adds HttpClient to it
        }

        static void Main(string[] args)
        {
            try
            { 
                Console.WriteLine($"Wolf.Notification.EmailSender Service starting in {AppContext.BaseDirectory}");
                CreateHostBuilder(args).Build().Run(); 
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Log.Fatal(ex, ex.Message);
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .UseWindowsService()
            .ConfigureAppConfiguration((hostBuilderContext, configurationBinder) =>
            {
                bool isWinSvc = WindowsServiceHelpers.IsWindowsService();
                Console.WriteLine($"\t Current Directory:  {Directory.GetCurrentDirectory()};\r\n\t Windows Service: {isWinSvc};\r\n\t AppContext.BaseDirectory: {AppContext.BaseDirectory};\r\n\t Env: {hostBuilderContext.HostingEnvironment.EnvironmentName}\r\n");
                Directory.SetCurrentDirectory(AppContext.BaseDirectory);
                configurationBinder.SetBasePath(AppContext.BaseDirectory);
            })
            .UseSerilog()
            .ConfigureServices((hostContext, services) =>
            {
                BuildDI(hostContext, services);
            });
    }
}
