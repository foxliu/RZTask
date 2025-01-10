namespace RZTask.Common.Structs
{
    public class HostInfoStruct
    {
        public string AppName { get; set; }

        public string HostName { get; set; }

        public string OsPlatform { get; set; }

        public List<Dictionary<string, List<string>>> IpAddresses { get; set; }

        public List<string> GatewayAddresses { get; set; }

        public List<string> DnsAddresses { get; set; }
    }
}
