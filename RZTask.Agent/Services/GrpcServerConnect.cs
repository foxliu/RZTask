using Grpc.Net.Client;
using RZTask.Common.Protos;
using RZTask.Common.Utils;
using System.Security.Cryptography.X509Certificates;
using ILogger = Serilog.ILogger;

namespace RZTask.Agent.Services
{
    public class GrpcServerConnect
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;
        public readonly String ServerUrl;

        public GrpcServerConnect(ILogger logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            ServerUrl = _configuration.GetSection("Server")["Url"] ?? "";
        }

        public ServerService.ServerServiceClient InitGrpcChannel()
        {
            var serverOptions = _configuration.GetSection("Server");
            try
            {
                var url = serverOptions["Url"]!;
                var configCertPath = serverOptions["Certificate:Path"]!;
                var certPath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, configCertPath);
                Console.WriteLine($"===================> cert path: {certPath}");

                var channel = GrpcClientService.CreateChannel(url, new X509Certificate2(certPath));
                return new ServerService.ServerServiceClient(channel);
            }
            catch (Exception ex)
            {
                _logger.Error($"Init grpc channel error: {ex.Message}");
                _logger.Error(ex.StackTrace ?? "");
                Console.Error.WriteLine(ex.Message);
                if (ex.StackTrace != null)
                {
                    _logger.Error(ex.StackTrace);
                }
                Environment.Exit(1);
                throw;
            }
        }
    }
}
