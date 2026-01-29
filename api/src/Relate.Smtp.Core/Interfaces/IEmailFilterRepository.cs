using Relate.Smtp.Core.Entities;

namespace Relate.Smtp.Core.Interfaces;

public interface IEmailFilterRepository
{
    Task<IReadOnlyList<EmailFilter>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EmailFilter>> GetEnabledByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<EmailFilter?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<EmailFilter> AddAsync(EmailFilter filter, CancellationToken cancellationToken = default);
    Task UpdateAsync(EmailFilter filter, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
