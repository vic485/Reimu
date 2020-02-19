using System.Security.Cryptography.X509Certificates;
using System.Text.Json.Serialization;
using Reimu.Common.Logging;

namespace Reimu.Common.Configuration
{
    public class LocalSettings
    {
        public LogType LogLevel { get; set; } = LogType.Info;
        public int Shard { get; set; } = 0;
        public int TotalShards { get; set; } = 1;
        public string DatabaseName { get; set; } = "Reimu";
        public string[] DatabaseUrls { get; set; } = {"http://localhost:8080"};
        public string CertificatePath { get; set; }

        public X509Certificate2 Certificate 
            => !string.IsNullOrWhiteSpace(CertificatePath) ? new X509Certificate2(CertificatePath) : null;
    }
}