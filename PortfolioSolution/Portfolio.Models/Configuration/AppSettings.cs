namespace Portfolio.Models.Configuration
{
    /// <summary>
    /// General application-level settings.
    /// Bind this from appsettings.json or environment variables.
    /// </summary>
    public class AppSettings
    {
        /// <summary>
        /// Publicly reachable base URL of this API (no trailing slash).
        /// Used to build absolute media download URLs.
        /// Example: "https://portfolio-backend-jsyz.onrender.com"
        /// </summary>
        public string PublicBaseUrl { get; set; } = string.Empty;
    }
}
