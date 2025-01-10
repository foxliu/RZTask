using Microsoft.Extensions.Configuration;
using RZTask.Common.Structs;
using Serilog;
using System.Diagnostics;
using System.Net.NetworkInformation;


namespace RZTask.Common.Utils
{
    public class LocalInfo
    {
        private readonly ILogger logger;
        private readonly IConfiguration configuration;

        private HostInfoStruct _hostInfo = new();

        public LocalInfo(ILogger logger, IConfiguration configuration)
        {
            this.logger = logger;
            this.configuration = configuration;
        }

        public HostInfoStruct GetHostInfoStruct()
        {
            try
            {
                if (!EqualityComparer<HostInfoStruct>.Default.Equals(_hostInfo, default)) return _hostInfo;

                var info = new HostInfoStruct
                {
                    HostName = Environment.MachineName,
                    OsPlatform = System.Runtime.InteropServices.RuntimeInformation.OSDescription,
                    AppName = GetAppName(""),
                    IpAddresses = new(),
                    DnsAddresses = new(),
                    GatewayAddresses = new()
                };

                GetNetworkInfo(ref info);

                _hostInfo = info;
                return info;
            }
            catch (Exception ex)
            {
                logger.Error($"Get local info error {ex.Message}");
                throw;
            }
        }

        private void GetNetworkInfo(ref HostInfoStruct hostInfo)
        {
            List<string> dnses = new();
            List<string> gatewaies = new();
            foreach (var networkInterface in NetworkInterface.GetAllNetworkInterfaces())
            {
                var ipProperties = networkInterface.GetIPProperties();
                hostInfo.IpAddresses.Add(new Dictionary<string, List<string>>()
                {
                    {
                        networkInterface.Name,
                        ipProperties.UnicastAddresses
                            .Where(ip => ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                            .Select(ip => ip.Address.ToString()).ToList()
                    }
                });
                dnses.AddRange(ipProperties.DnsAddresses
                    .Where(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    .Select(dns => dns.ToString()).ToList());
                gatewaies.AddRange(ipProperties.GatewayAddresses
                    .Where(ip => ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    .Select(gw => gw.Address.ToString()).ToList());
            }

            hostInfo.DnsAddresses = dnses.Distinct().ToList();

            hostInfo.GatewayAddresses = gatewaies.Distinct().ToList();
        }

        public string? GetIpByInterfaceName(string interfaceName)
        {
            try
            {
                return NetworkInterface.GetAllNetworkInterfaces()
                        .Where(_interface => _interface.Name == interfaceName)
                        .Select(_interface =>
                        {
                            return _interface.GetIPProperties().UnicastAddresses
                            .Where(ip => ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                            .Select(ip => ip.Address.ToString()).First();
                        }).First();
            }
            catch (Exception ex)
            {
                logger.Error($"Get ip by interface name [{interfaceName}] error: {ex.Message}, return 255.255.255.255");
                return "255.255.255.255";
            }
        }

        public string? GetBusiIp()
        {
            string ip = "127.0.0.1";
            string? busiInterfaceName = Environment.GetEnvironmentVariable(configuration.GetSection("Agent:default_interface").Value ?? "eth0");
            if (!string.IsNullOrEmpty(busiInterfaceName)) { ip = this.GetIpByInterfaceName(busiInterfaceName) ?? ""; }

            if (!string.IsNullOrEmpty(ip)) { return ip.ToString(); }

            var hostInfo = this.GetHostInfoStruct();

            string ipAddr = "127.0.0.1";
            if (hostInfo.IpAddresses.Count > 0)
            {
                foreach (var ipInfo in hostInfo.IpAddresses)
                {
                    if (ipInfo.ContainsKey("eth0"))
                    {
                        var ipList = ipInfo.GetValueOrDefault("eth0");
                        ipAddr = ipList == null ? string.Empty : ipList.First();
                        return ipAddr;
                    }
                    else
                    {
                        var ipList = ipInfo.Values.First();
                        ipAddr = ipList == null ? "127.0.0.1" : ipList.First();
                    }
                }
            }
            return ipAddr;
        }

        public string GetBusiIp(string defaultIp)
        {
            var busiIp = this.GetBusiIp();
            return string.IsNullOrEmpty(busiIp) ? defaultIp : busiIp;
        }

        public string GetAppName(string defaultAppname)
        {
            var environmentName = "APP_NAME";
            // First get info from current process environment
            var appName = Environment.GetEnvironmentVariable(environmentName);

            if (!string.IsNullOrEmpty(appName))
            {
                return appName;
            }

            // Get info from pid == 1 process environment
            int pid = 1;
            try
            {
                Process process = Process.GetProcessById(pid);

                appName = process.StartInfo.EnvironmentVariables[environmentName];

                if (!string.IsNullOrEmpty(appName))
                {
                    return appName;
                }
                return defaultAppname;
            }
            catch (Exception ex)
            {
                logger.Error($"Get app_name error: {ex.Message}, return default app_name: {defaultAppname}");
                return defaultAppname;
            }
        }
    }
}
