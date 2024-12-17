using RZTask.Common.Protos;
using RZTask.Common.Structs;
using RZTask.Common.Utils;

namespace RZTask.Agent.Services
{
    public class HeartbeatkBackgroundService : BackgroundService
    {
        private readonly Serilog.ILogger _logger;
        private readonly IConfiguration _configuration;
        private readonly AgentRegistrar _agentRegistrar;
        private readonly Timer? _timer = null;
        private readonly LocalInfo _localInfo;
        private CertificateInfo _certificateInfo;

        public HeartbeatkBackgroundService(Serilog.ILogger logger, IConfiguration configuration,
            AgentRegistrar agentRegistrar, LocalInfo localInfo, CertificateInfo certificateInfo)
        {
            _configuration = configuration;
            _logger = logger;
            _agentRegistrar = agentRegistrar;
            _localInfo = localInfo;
            _certificateInfo = certificateInfo;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.Information("Start initialization key and certificate file ...");
            var result = await KeyFileInitializeServiceAsync();

            while (!stoppingToken.IsCancellationRequested && !result)
            {
                await Task.Delay(10000);

                result = await KeyFileInitializeServiceAsync();
            }
            while (!stoppingToken.IsCancellationRequested)
            {
                await SendHeartbeat();

                await Task.Delay(TimeSpan.FromSeconds(_configuration.GetValue<int>("Server:HealthInterval")), stoppingToken);
            }
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.Information($"HeartbeatService stoping...");
            return Task.CompletedTask;
        }

        private async Task SendHeartbeat()
        {
            try
            {
                var localIp = _localInfo.GetIpByInterfaceName("以太网") ?? "255.255.255.255";

                // Generate agent grpc url
                var grpcUrl = _configuration.GetSection("Kestrel:Endpoints:Grpc:Url").Value!;

                Response response = await _agentRegistrar.AgentHeartbeatAsync(localIp);
                switch (response.Code)
                {
                    case 200:
                        _logger.Information($"Send heartbeat to server: {_agentRegistrar.ServerUrl} success");
                        return;
                    case 417:
                        _logger.Information($"{response.Message}");
                        var appName = _localInfo.GetAppName("test");

                        await _agentRegistrar.RegisterAsync(localIp, grpcUrl, appName, _certificateInfo.KeyData, _certificateInfo.CertData);
                        return;
                    default:
                        _logger.Error($"Send heartbeat to server: {_agentRegistrar.ServerUrl} failed: {response.Code}:{response.Message}");
                        return;
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Send health info error: {ex.Message}");
                if (ex.StackTrace != null)
                {
                    _logger.Error(ex.StackTrace);
                }
            }
        }

        private async Task<bool> KeyFileInitializeServiceAsync()
        {
            var grpcUrl = _configuration["Kestrel:Endpoints:Grpc:Url"];
            try
            {
                var _keyFilePath = _configuration["Kestrel:Endpoints:Grpc:Certificate:KeyPath"];
                var _certFilePath = _configuration["Kestrel:Endpoints:Grpc:Certificate:Store"];

                var generator = new KeyAndCertGenerator(_keyFilePath, _certFilePath);
                generator.GenerateKeyAndCert(ref _certificateInfo);

                _logger.Information($"key and certificate inited, cert file path: {_certFilePath}");

                var localIp = _localInfo.GetIpByInterfaceName("以太网") ?? "255.255.255.255";

                var appName = _localInfo.GetAppName("test");

                var response = await _agentRegistrar.RegisterAsync(localIp, grpcUrl!, appName, _certificateInfo.KeyData, _certificateInfo.CertData);

                if (response.Code == 200)
                {
                    Console.WriteLine("Init Cert File And Registrat Succfull");
                    return true;
                }
                else
                {
                    Console.WriteLine($"向Server注册Agent: {grpcUrl}, 错误: {response.Message}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"key and certificate init failed: {ex.Message}");
                Console.WriteLine($"向Server注册Agent: {grpcUrl}, 错误: {ex.Message}");
                if (ex.StackTrace != null)
                {
                    _logger.Error(ex.StackTrace);
                }
                return false;
            }
        }
    }
}
