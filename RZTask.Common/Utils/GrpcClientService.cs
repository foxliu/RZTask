using Grpc.Net.Client;
using System.Security.Cryptography.X509Certificates;

namespace RZTask.Common.Utils
{
    public static class GrpcClientService
    {
        public static GrpcChannel CreateChannel(string url, string certificatePath, string password)
        {
            string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string certPath = Path.Combine(currentDirectory, certificatePath);
            var certificate = new X509Certificate2(certPath, password);

            var httpHandler = new HttpClientHandler();
            httpHandler.ClientCertificates.Add(certificate);

            httpHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) =>
            {
                var store = ApplicationStore.Instance;

                return sslPolicyErrors == System.Net.Security.SslPolicyErrors.None || cert!.Thumbprint.Equals(store.Thumbprint, StringComparison.OrdinalIgnoreCase);
            };

            var channel = GrpcChannel.ForAddress(url, new GrpcChannelOptions
            {
                HttpClient = new HttpClient(httpHandler),
            });

            return channel;
        }

        public static GrpcChannel CreateChannel(string url, X509Certificate2 certificate)
        {
            var httpHandler = new HttpClientHandler();
            httpHandler.ClientCertificates.Add(certificate);

            httpHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) =>
            {
                if (sslPolicyErrors == System.Net.Security.SslPolicyErrors.None)
                {
                    return true;
                }

                return cert != null && cert.Thumbprint.Equals(certificate.Thumbprint, StringComparison.OrdinalIgnoreCase);
            };

            var channel = GrpcChannel.ForAddress(url, new GrpcChannelOptions
            {
                HttpClient = new HttpClient(httpHandler),
            });

            return channel;
        }
    }
}
