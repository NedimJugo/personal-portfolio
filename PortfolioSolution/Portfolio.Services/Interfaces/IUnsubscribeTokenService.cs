using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portfolio.Services.Interfaces
{
    public interface IUnsubscribeTokenService
    {
        string GenerateToken(string email);
        string DecryptEmail(string token);
    }
}
