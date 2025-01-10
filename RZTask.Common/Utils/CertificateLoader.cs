using System.Reflection;
using System.Security.Cryptography.X509Certificates;

namespace RZTask.Common.Utils
{
    public class CertificateLoader
    {
        public X509Certificate2 LoadCertificateFromEmbeddedResource(string resourceName, string certificatePassword)
        {
            var assembly = Assembly.GetExecutingAssembly();

            using var stream = assembly.GetManifestResourceStream(resourceName) ?? throw new FileNotFoundException("Certificate resource file not found", resourceName);

            try
            {
                return new X509Certificate2(ReadToEnd(stream), certificatePassword);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to load the certificate.", ex);
            }
        }

        private byte[] ReadToEnd(Stream stream)
        {
            if (stream == null || stream.Length == 0)
            {
                throw new ArgumentException("Stream is null or empty", nameof(stream));
            }

            stream.Seek(0, SeekOrigin.Begin);

            using var memoryStream = new MemoryStream();
            stream.CopyTo(memoryStream);

            return memoryStream.ToArray();
        }
    }
}
