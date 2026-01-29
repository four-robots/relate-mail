using Relate.Smtp.Core.Entities;

namespace Relate.Smtp.Core.Interfaces;

public interface ILabelRepository
{
    Task<IReadOnlyList<Label>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Label?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Label> AddAsync(Label label, CancellationToken cancellationToken = default);
    Task UpdateAsync(Label label, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
