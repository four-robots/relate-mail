using Relate.Smtp.Core.Entities;

namespace Relate.Smtp.Api.Models;

public record LabelDto(
    Guid Id,
    string Name,
    string Color,
    int SortOrder,
    DateTimeOffset CreatedAt
);

public record CreateLabelRequest(
    string Name,
    string Color,
    int SortOrder = 0
);

public record UpdateLabelRequest(
    string? Name,
    string? Color,
    int? SortOrder
);

public static class LabelExtensions
{
    public static LabelDto ToDto(this Label label)
    {
        return new LabelDto(
            label.Id,
            label.Name,
            label.Color,
            label.SortOrder,
            label.CreatedAt
        );
    }
}
