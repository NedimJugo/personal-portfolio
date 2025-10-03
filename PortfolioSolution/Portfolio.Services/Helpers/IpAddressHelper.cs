using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Portfolio.Services.Helpers
{
    public static class IpAddressHelper
    {
        public static string? GetClientIpAddress(HttpContext httpContext)
        {
            // Check for X-Forwarded-For header (proxy/load balancer)
            var forwardedFor = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                // Take the first IP if there are multiple
                var ips = forwardedFor.Split(',');
                return ips[0].Trim();
            }

            // Check for X-Real-IP header
            var realIp = httpContext.Request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrEmpty(realIp))
            {
                return realIp;
            }

            // Fall back to RemoteIpAddress
            return httpContext.Connection.RemoteIpAddress?.ToString();
        }
    }
}
