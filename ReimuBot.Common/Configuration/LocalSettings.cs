using System.Security.Cryptography.X509Certificates;
using Reimu.Common.Logging;

namespace Reimu.Common.Configuration
{
    public class LocalSettings
    {
        /// <summary>
        /// Lowest severity log messages we send to console/file
        /// </summary>
        public LogType LogLevel { get; set; } = LogType.Info;

        /// <summary>
        /// Total number of shards the bot will have
        /// </summary>
        public int TotalShards { get; set; } = 1;

        /// <summary>
        /// Name of the database to access
        /// </summary>
        public string DatabaseName { get; set; } = "Reimu";

        /// <summary>
        /// Urls to access database nodes
        /// Must be http(s):// fqdn OR ip:port
        /// </summary>
        public string[] DatabaseUrls { get; set; } = {"http://localhost:8080"};

        /// <summary>
        /// Path to certificate file (.pfx) on disk
        /// </summary>
        public string CertificatePath { get; set; }

        /// <summary>
        /// X509 certificate to authenticate with the database, if required
        /// </summary>
        public X509Certificate2 Certificate
            => !string.IsNullOrWhiteSpace(CertificatePath) ? new X509Certificate2(CertificatePath) : null;
    }
}
