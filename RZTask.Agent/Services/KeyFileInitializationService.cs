using RZTask.Common.Structs;
using RZTask.Common.Utils;

namespace RZTask.Agent.Services
{
    public class KeyFileInitializationService : IHostedService
    {
        private readonly string? _keyFilePath;
        private readonly string? _certFilePath;
        private CertificateInfo _certificateInfo;
        private readonly Serilog.ILogger _logger;
        private readonly IConfiguration _configuration;
        private readonly LocalInfo _localInfo;
        private readonly AgentRegistrar _agentRegistrar;

        public KeyFileInitializationService(Serilog.ILogger logger, IConfiguration configuration, CertificateInfo certificateInfo, LocalInfo localInfo, AgentRegistrar agentRegistrar)
        {
            _keyFilePath = configuration["Kestrel:Endpoints:Grpc:Certificate:KeyPath"];
            _certFilePath = configuration["Kestrel:Endpoints:Grpc:Certificate:Store"];
            _certificateInfo = certificateInfo;
            _logger = logger;
            _localInfo = localInfo;
            _agentRegistrar = agentRegistrar;
            _configuration = configuration;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.Information("Start initialization key and certificate file ...");
            try
            {
                var generator = new KeyAndCertGenerator(_keyFilePath, _certFilePath);
                generator.GenerateKeyAndCert(ref _certificateInfo);
                _logger.Information($"-------------------> {_certificateInfo.KeyData}");
                _logger.Information($"key and certificate inited, cert file path: {_certFilePath}");

                var localIp = _localInfo.GetIpByInterfaceName("以太网") ?? "255.255.255.255";

                // Generate agent grpc url
                var grpcUrl = string.Format($"https://{localIp}:5001");

                var appName = _localInfo.GetAppName("test");

                var response = _agentRegistrar.RegisterAsync(localIp, grpcUrl, appName, _certificateInfo.KeyData, _certificateInfo.CertData).GetAwaiter().GetResult();

                if (response.Code == 200)
                {
                    Console.WriteLine("Init Cert File And Registrat Succfull");
                }
                else
                {
                    Console.WriteLine($"Init Cert File And Registrat Failed: {response.Message}");
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"key and certificate init failed: {ex.Message}");
                Console.WriteLine(ex.ToString());
                if (ex.StackTrace != null)
                {
                    _logger.Error(ex.StackTrace);
                }
            }

            
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancelToken)
        {
            return Task.CompletedTask;
        }
    }
}
