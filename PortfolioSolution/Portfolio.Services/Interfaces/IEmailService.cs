using Portfolio.Models.Responses;
using Portfolio.Services.Database.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portfolio.Services.Interfaces
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default);
        Task<bool> SendReplyEmailAsync(ContactMessageReply reply, CancellationToken cancellationToken = default);
        Task<List<ReceivedEmailResponse>> FetchNewEmailsAsync(CancellationToken cancellationToken = default);
        Task<bool> MarkEmailAsReadAsync(string messageId, CancellationToken cancellationToken = default);
    }
}
