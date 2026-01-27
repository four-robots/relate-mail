using System.Buffers;
using MimeKit;
using SmtpServer;
using SmtpServer.Protocol;
using SmtpServer.Storage;
using Relate.Smtp.Core.Entities;
using Relate.Smtp.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace Relate.Smtp.SmtpHost.Handlers;

public class CustomMessageStore : MessageStore
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CustomMessageStore> _logger;

    public CustomMessageStore(IServiceProvider serviceProvider, ILogger<CustomMessageStore> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public override async Task<SmtpResponse> SaveAsync(
        ISessionContext context,
        IMessageTransaction transaction,
        ReadOnlySequence<byte> buffer,
        CancellationToken cancellationToken)
    {
        try
        {
            using var stream = new MemoryStream();
            foreach (var segment in buffer)
            {
                stream.Write(segment.Span);
            }
            stream.Position = 0;

            var message = await MimeMessage.LoadAsync(stream, cancellationToken);

            using var scope = _serviceProvider.CreateScope();
            var emailRepository = scope.ServiceProvider.GetRequiredService<IEmailRepository>();

            var email = new Email
            {
                Id = Guid.NewGuid(),
                MessageId = message.MessageId ?? Guid.NewGuid().ToString(),
                FromAddress = message.From.Mailboxes.FirstOrDefault()?.Address ?? string.Empty,
                FromDisplayName = message.From.Mailboxes.FirstOrDefault()?.Name,
                Subject = message.Subject ?? "(No Subject)",
                TextBody = message.TextBody,
                HtmlBody = message.HtmlBody,
                ReceivedAt = DateTimeOffset.UtcNow,
                SizeBytes = buffer.Length
            };

            // Add recipients
            AddRecipients(email, message.To, RecipientType.To);
            AddRecipients(email, message.Cc, RecipientType.Cc);
            AddRecipients(email, message.Bcc, RecipientType.Bcc);

            // Add attachments
            foreach (var attachment in message.Attachments)
            {
                if (attachment is MimePart mimePart)
                {
                    using var attachmentStream = new MemoryStream();
                    await mimePart.Content.DecodeToAsync(attachmentStream, cancellationToken);

                    email.Attachments.Add(new EmailAttachment
                    {
                        Id = Guid.NewGuid(),
                        EmailId = email.Id,
                        FileName = mimePart.FileName ?? "attachment",
                        ContentType = mimePart.ContentType.MimeType,
                        SizeBytes = attachmentStream.Length,
                        Content = attachmentStream.ToArray()
                    });
                }
            }

            await emailRepository.AddAsync(email, cancellationToken);

            _logger.LogInformation("Email saved: {MessageId} from {From} to {Recipients}",
                email.MessageId,
                email.FromAddress,
                string.Join(", ", email.Recipients.Select(r => r.Address)));

            return SmtpResponse.Ok;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save email");
            return SmtpResponse.TransactionFailed;
        }
    }

    private static void AddRecipients(Email email, InternetAddressList addresses, RecipientType type)
    {
        foreach (var address in addresses.Mailboxes)
        {
            email.Recipients.Add(new EmailRecipient
            {
                Id = Guid.NewGuid(),
                EmailId = email.Id,
                Address = address.Address,
                DisplayName = address.Name,
                Type = type,
                IsRead = false
            });
        }
    }
}
