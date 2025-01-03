using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using RZTask.Common.Protos;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Serilog;
using System;
using System.Net.Security;
using RZTask.Common.Utils;

namespace RZTask.Agent
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                Exception exception = (Exception)e.ExceptionObject;
                Console.WriteLine($"Unhandled exception: {exception.Message}");
            };

            TaskScheduler.UnobservedTaskException += (sender, e) =>
            {
                Console.WriteLine($"Unhandled exception: {e.Exception.Message}");
                e.SetObserved();
            };

            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, config) =>
            {
                config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

                var env = context.HostingEnvironment;
                config.AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);
            })
            .UseSerilog((contex, config) =>
            {
                config.ReadFrom.Configuration(contex.Configuration);
            })
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseKestrel((context, option) =>
                {
                    option.Configure(context.Configuration.GetSection("Kestrel"), reloadOnChange: true);
                });

                webBuilder.UseStartup<Startup>();
            });
    }
}
