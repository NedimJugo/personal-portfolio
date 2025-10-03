using Microsoft.Extensions.Logging;
using Portfolio.Models.Responses;
using Portfolio.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Portfolio.Services.Services
{
    public class GeolocationService : IGeolocationService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<GeolocationService> _logger;

        public GeolocationService(HttpClient httpClient, ILogger<GeolocationService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<(string? Country, string? City)> GetLocationFromIpAsync(string ipAddress)
        {
            // Skip for localhost/private IPs
            if (string.IsNullOrEmpty(ipAddress) ||
                ipAddress == "::1" ||
                ipAddress.StartsWith("127.") ||
                ipAddress.StartsWith("192.168.") ||
                ipAddress.StartsWith("10."))
            {
                return ("Local", "Local");
            }

            try
            {
                // Using free ip-api.com service (100 requests/minute limit)
                var response = await _httpClient.GetStringAsync($"http://ip-api.com/json/{ipAddress}");
                var data = JsonSerializer.Deserialize<IpApiResponse>(response);

                if (data?.Status == "success")
                {
                    return (data.Country, data.City);
                }

                return (null, null);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get geolocation for IP: {IpAddress}", ipAddress);
                return (null, null);
            }
        }
    }
}