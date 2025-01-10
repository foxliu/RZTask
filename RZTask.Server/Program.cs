using Microsoft.AspNetCore.Server.Kestrel.Https;
using RZTask.Common.Utils;
using Serilog;

namespace RZTask.Server
{
    public class Program
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
                config.AddJsonFile($"appsettings.{env.EnvironmentName}.json");
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

                    option.ConfigureHttpsDefaults(httpsOptions =>
                    {
                        httpsOptions.ClientCertificateMode = ClientCertificateMode.AllowCertificate;

                        httpsOptions.ClientCertificateValidation = (cert, chain, sslPolicyErrors) =>
                        {
                            var store = ApplicationStore.Instance;
                            return cert.Thumbprint.Equals(store.Thumbprint, StringComparison.OrdinalIgnoreCase);
                        };
                    });
                });

                webBuilder.UseStartup<Startup>();
            });
    }
}