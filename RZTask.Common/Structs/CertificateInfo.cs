using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RZTask.Common.Structs
{
    public class CertificateInfo
    {
        public byte[] Certificate { get; set; }
        public byte[] PrivateKey { get; set; }
        public string Thumbprint { get; set; }
    }
}
