namespace Domain.Models;

public record PaginationModel<T>(IEnumerable<T> List, int PageCount);
