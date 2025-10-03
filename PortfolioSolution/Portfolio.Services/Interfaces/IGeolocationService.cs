using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portfolio.Services.Interfaces
{
    public interface IGeolocationService
    {
        Task<(string? Country, string? City)> GetLocationFromIpAsync(string ipAddress);
    }

}
