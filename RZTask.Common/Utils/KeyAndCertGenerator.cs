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
            _certFilePath = certFilePath ?? "/etc/rstask/pki/agent.crt";
        }

        public void GenerateKeyAndCert()
        {
            if (File.Exists(_keyFilePath) && File.Exists(_certFilePath))
            {
                var privateKey = File.ReadAllText(_keyFilePath);
                var certificate = File.ReadAllBytes(_certFilePath);

                var store = CertificateStore.Instance;
                store.Certificate = new X509Certificate2(certificate);
                store.PrivateKey = privateKey;

                SaveThumprint(_keyFilePath, store.Thumbprint!);
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

            // 1. 创建 RSA 密钥对
            using (var rsa = RSA.Create(2048))
            {
                // 2. 创建证书请求
                var certificateRequest = new CertificateRequest(
                    "CN=RZTask;OU=PaaS;O=RZTask;L=Shanghai;S = Shanghai;C=CN",
                    rsa,
                    HashAlgorithmName.SHA256,
                    RSASignaturePadding.Pkcs1);

                certificateRequest.CertificateExtensions.Add(
                    new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.KeyEncipherment, false)
                    );

                var basicConstraints = new X509BasicConstraintsExtension(
                    certificateAuthority: false,
                    hasPathLengthConstraint: false,
                    pathLengthConstraint: 0,
                    critical: false);
                certificateRequest.CertificateExtensions.Add(basicConstraints);

                certificateRequest.CertificateExtensions.Add(
                    new X509SubjectKeyIdentifierExtension(certificateRequest.PublicKey, false)
                    );

                // 3. 生成自签名证书（有效期10年）
                var cert = certificateRequest.CreateSelfSigned(DateTime.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddYears(10));

                // 4. 导出证书（保存为 .crt 文件）
                File.WriteAllText(_certFilePath, cert.ExportCertificatePem());

                // 5. 导出私钥 （保存为 .key 文件）
                File.WriteAllText(_keyFilePath, rsa.ExportRSAPrivateKeyPem());

                var store = CertificateStore.Instance;
                store.Certificate = cert;
                store.PrivateKey = rsa.ExportRSAPrivateKeyPem();

                SaveThumprint(_keyFilePath, cert.Thumbprint);   
            }
        }

        private void SaveThumprint(string keyPath, string thumprint)
        {
            var thumprintPath = Path.Combine(Path.GetDirectoryName(_keyFilePath) ?? System.AppDomain.CurrentDomain.BaseDirectory, "thumbprint");
            File.WriteAllText(thumprintPath, thumprint);
        }
    }
}
