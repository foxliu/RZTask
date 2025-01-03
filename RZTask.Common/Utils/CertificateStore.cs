using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace RZTask.Common.Utils
{
    public class CertificateStore
    {
        private X509Certificate2? _certificate;
        private string? _thumbprint;
        private string? _privateKey;
        private string? _certificateByte;

        private static readonly Lazy<CertificateStore> _instance = new Lazy<CertificateStore>(() => new CertificateStore());

        private CertificateStore() { }

        public static CertificateStore Instance => _instance.Value;

        public X509Certificate2? Certificate
        {
            get => _certificate;
            set
            {
                _certificate = value;
                _thumbprint = value?.Thumbprint;
                _certificateByte = value?.ExportCertificatePem();
            }
        }

        public string? Thumbprint => _thumbprint;
        public string? CertificateByte => _certificateByte;
        public string? PrivateKey
        {
            get => _privateKey;
            set => _privateKey = value;
        }

        public void Clear()
        {
            _certificate = null;
            _thumbprint = null;
            _privateKey = null;
        }
    }
}
