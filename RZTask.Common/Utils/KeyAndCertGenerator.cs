using RZTask.Common.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace RZTask.Common.Utils
{
    public class KeyAndCertGenerator
    {
        private readonly string _keyFilePath;
        private readonly string _certFilePath;

        public KeyAndCertGenerator(string? keyFilePath, string? certFilePath)
        {
            _keyFilePath = keyFilePath ?? "/etc/rstask/pki/agent.key";
            _certFilePath = certFilePath ?? "/etc/rstask/pki/agent.cert";
        }

        public void GenerateKeyAndCert(ref CertificateInfo certificateInfo)
        {
            if (File.Exists(_keyFilePath) && File.Exists(_certFilePath))
            {
                certificateInfo.PrivateKey = File.ReadAllBytes(_keyFilePath);
                certificateInfo.Certificate = File.ReadAllBytes(_certFilePath);
                return;
            }

            if (!Directory.Exists(Path.GetDirectoryName(_keyFilePath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_keyFilePath)!);
            }

            if (!Directory.Exists(Path.GetDirectoryName(_certFilePath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_certFilePath)!);
            }

            using (var rsa = RSA.Create(2048))
            {
                var certificateRequest = new CertificateRequest(
                    new X500DistinguishedName("CN=RZTaskAgent"), rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

                var cert = certificateRequest.CreateSelfSigned(DateTime.UtcNow, DateTimeOffset.UtcNow.AddYears(10));

                // write to key file
                var privateKey = rsa.ExportRSAPrivateKey();
                var privateCert = cert.Export(X509ContentType.Cert);

                certificateInfo.PrivateKey = privateKey;
                certificateInfo.Certificate = privateCert;

                File.WriteAllBytes(_keyFilePath, privateKey);

                File.WriteAllBytes(_certFilePath, privateCert);
            }

        }
    }
}
