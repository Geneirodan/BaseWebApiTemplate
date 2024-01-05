namespace BusinessLogic.Models;

public record PaginationModel<T>(IEnumerable<T> List, int PageCount);
