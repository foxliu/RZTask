using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

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
                Console.WriteLine($"SSL Policy Errors: {sslPolicyErrors}");
                //if (sslPolicyErrors == System.Net.Security.SslPolicyErrors.None)
                //{
                //    Console.WriteLine("No SSL Policy Errors.");
                //    return true;
                //}

                Console.WriteLine($"Comparing Thumbprint: Server: {cert.Thumbprint} Request: {certificate.Thumbprint}");
                if (cert.Thumbprint.Equals(certificate.Thumbprint, StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("Thumbprint matched.");
                    return true;
                }
                Console.WriteLine("Thumbprint did not match.");
                return false;
            };

            var channel = GrpcChannel.ForAddress(url, new GrpcChannelOptions
            {
                HttpClient = new HttpClient(httpHandler),
            });

            return channel;
        }
    }
}
