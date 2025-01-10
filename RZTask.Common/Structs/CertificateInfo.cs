namespace RZTask.Common.Structs
{
    public class CertificateInfo
    {
        public byte[] Certificate { get; set; }
        public byte[] PrivateKey { get; set; }
        public string Thumbprint { get; set; }
    }
}
