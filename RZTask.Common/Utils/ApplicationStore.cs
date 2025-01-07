using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace RZTask.Common.Utils
{
    public class ApplicationStore
    {
        private X509Certificate2? _certificate;
        private string? _thumbprint;
        private string? _privateKey;
        private string? _certificateByte;
        private readonly string _dllDirectory;
        private readonly string _execableFileDirectory;

        private static readonly Lazy<ApplicationStore> _instance = new Lazy<ApplicationStore>(() => new ApplicationStore());

        private ApplicationStore() {
            _dllDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dll");
            if (!Directory.Exists(_dllDirectory))
            {
                Directory.CreateDirectory(_dllDirectory);
            }

            _execableFileDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "execable_file");
            if (!Directory.Exists(_execableFileDirectory))
            {
                Directory.CreateDirectory(_execableFileDirectory);
            }
        }

        public static ApplicationStore Instance => _instance.Value;

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

        public string DllDirectory => _dllDirectory;
        public string ExecableFileDirectory => _execableFileDirectory;

        public void Clear()
        {
            _certificate = null;
            _thumbprint = null;
            _privateKey = null;
        }
    }
}
