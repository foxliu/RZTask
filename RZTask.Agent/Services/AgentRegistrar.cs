using Google.Protobuf;
using Grpc.Net.Client;
using RZTask.Common.Protos;
using ILogger = Serilog.ILogger;

namespace RZTask.Agent.Services
{
    public class AgentRegistrar
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;
        private readonly ServerService.ServerServiceClient _client;
        public readonly string ServerUrl;

        public AgentRegistrar(ILogger logger, IConfiguration configuration, GrpcServerConnect grpcServer)
        {
            _logger = logger;
            _configuration = configuration;

            _client = grpcServer.InitGrpcChannel();
            ServerUrl = grpcServer.ServerUrl;
        }

        public async Task<Response> RegisterAsync(string agentId, string grpcAddress, string appName, string privateKey, string certificate)
        {
            var request = new AgentRegistrationRequest
            {
                AgentId = agentId,
                GrpcAddress = grpcAddress,
                AppName = appName,
                PrivateKey = privateKey,
                Certificate = certificate
            };

            var response = await _client.RegisterAgentAsync(request);
            if (response.Code == 200)
            {
                _logger.Information("Agent registered successfully!");
            }
            else
            {
                _logger.Information($"Registration failed: {response.Message}");
            }
            return response;
        }

        public async Task<Response> AgentHeartbeatAsync(string agentId)
        {
            var request = new AgentHeartbeatRequest
            {
                AgentId = agentId,
            };

            var response = await _client.AgentHeartbeatAsync(request);
            return response;
        }
    }
}
