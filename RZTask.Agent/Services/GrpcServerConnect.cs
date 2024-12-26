using Grpc.Net.Client;
using RZTask.Common.Protos;
using RZTask.Common.Utils;
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
                var channel = GrpcClientService.CreateChannel(
                    serverOptions["Url"]!,
                    serverOptions["Certificate:Store"]!,
                    serverOptions["Certificate:Password"]!
                    );
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
