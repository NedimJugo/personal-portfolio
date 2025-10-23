using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portfolio.Services.Interfaces
{
    public interface IEmailSyncService
    {
        Task SyncEmailsAsync(CancellationToken cancellationToken = default);
        Task<int> ImportEmailAsContactMessageAsync(string messageId, CancellationToken cancellationToken = default);
    }
}
