using System.Security.Cryptography.X509Certificates;
using Newtonsoft.Json;

namespace Reimu.Core.JsonModels
{
    public class ReimuSettings
    {
        public int Shard { get; set; } = 0;
        public int TotalShards { get; set; } = 1;
        public string DatabaseName { get; set; } = "Reimu";
        public string[] Urls { get; set; } = {"http://127.0.0.1:8080"};
        public string CertificatePath { get; set; }
        
        [JsonIgnore]
        public X509Certificate2 Certificate
             => !string.IsNullOrWhiteSpace(CertificatePath) ? new X509Certificate2(CertificatePath) : null;
    }
}