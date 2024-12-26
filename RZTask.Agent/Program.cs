using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using RZTask.Common.Protos;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Serilog;

namespace RZTask.Agent
{
    internal class Program
    {
        public static void Main(string[] args)
        {
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
                webBuilder.UseKestrel((context, options) =>
                {
                    options.ConfigureHttpsDefaults(httpsOptions =>
                    {
                        httpsOptions.ClientCertificateMode = ClientCertificateMode.RequireCertificate;

                        httpsOptions.ClientCertificateValidation = (cert, chain, sslPoliceErrors) =>
                        {
                            var keyPath = context.Configuration["Kestrel:Endpoints:Grpc:Certificate:KeyPath"];
                            var thumbprintPath = Path.Combine(keyPath ?? System.AppDomain.CurrentDomain.BaseDirectory, "thumbprint");
                            var thumbprint = File.ReadAllText(thumbprintPath);

                            return cert.Thumbprint.Equals(thumbprint, StringComparison.OrdinalIgnoreCase);
                        };
                    });
                });

                webBuilder.UseStartup<Startup>();
            });
    }
}
