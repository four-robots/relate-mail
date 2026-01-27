using Relate.Smtp.Core.Entities;

namespace Relate.Smtp.Core.Interfaces;

public interface IEmailRepository
{
    Task<Email?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Email?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Email>> GetByUserIdAsync(Guid userId, int skip, int take, CancellationToken cancellationToken = default);
    Task<int> GetCountByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<int> GetUnreadCountByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Email> AddAsync(Email email, CancellationToken cancellationToken = default);
    Task UpdateAsync(Email email, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task LinkEmailsToUserAsync(Guid userId, IEnumerable<string> emailAddresses, CancellationToken cancellationToken = default);
}
