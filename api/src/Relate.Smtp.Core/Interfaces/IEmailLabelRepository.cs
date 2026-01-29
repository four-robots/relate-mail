using Relate.Smtp.Core.Entities;

namespace Relate.Smtp.Core.Interfaces;

public interface IEmailLabelRepository
{
    Task<IReadOnlyList<EmailLabel>> GetByEmailIdAsync(Guid emailId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Email>> GetEmailsByLabelIdAsync(Guid userId, Guid labelId, int skip, int take, CancellationToken cancellationToken = default);
    Task<int> GetEmailCountByLabelIdAsync(Guid userId, Guid labelId, CancellationToken cancellationToken = default);
    Task<EmailLabel> AddAsync(EmailLabel emailLabel, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid emailId, Guid labelId, CancellationToken cancellationToken = default);
}
