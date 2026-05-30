namespace GymAdmin.Application.DTOs.Common;

public record PagedResult<T>(
    IReadOnlyList<T> Items,
    int TotalCount,
    int? Page = null,
    int? PageSize = null
);
