using AutoFilterer.Attributes;
using AutoFilterer.Enums;
using AutoFilterer.Types;

namespace BusinessLogic.Models.Filters;

public class UserFilter : PaginationFilterBase
{
    public string? Id { get; set; }
    
    [StringFilterOptions(StringFilterOption.Contains)]
    public string? UserName { get; set; }

    [StringFilterOptions(StringFilterOption.Contains)]
    public string? Email { get; set; }

    public bool? EmailConfirmed { get; set; }
}
