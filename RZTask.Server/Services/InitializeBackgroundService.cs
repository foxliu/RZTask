using RZTask.Common.Utils;

namespace RZTask.Server.Services
{
    public class InitializeBackgroundService : IHostedService
    {
        private readonly Serilog.ILogger _logger;
        private readonly IConfiguration _configuration;

        public InitializeBackgroundService(Serilog.ILogger logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            ReadCertificateInfoAsync();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private void ReadCertificateInfoAsync()
        {
            try
            {
                var _keyFilePath = _configuration["Kestrel:Endpoints:Grpc:Certificate:KeyPath"];
                var _certFilePath = _configuration["Kestrel:Endpoints:Grpc:Certificate:Path"];

                var keyAndCertGenerator = new KeyAndCertGenerator(_keyFilePath, _certFilePath);
                keyAndCertGenerator.GenerateKeyAndCert();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
