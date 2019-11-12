using System.Security.Cryptography.X509Certificates;

namespace Reimu.Core.Configuration
{
    public class SettingData
    {
        public int Shard { get; set; } = 0;
        public int TotalShards { get; set; } = 1;
        public string DatabaseName { get; set; } = "Reimu";
        public string[] Urls { get; set; } = {"http://localhost:8080"};
        public string CertificatePath { get; set; }

        public X509Certificate2 Certificate =>
            !string.IsNullOrWhiteSpace(CertificatePath) ? new X509Certificate2(CertificatePath) : null;
    }
}