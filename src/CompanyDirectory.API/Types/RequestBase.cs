using System.ComponentModel;

namespace CompanyDirectory.API.Types;

public class PagedRequest
{
    [DefaultValue(0)]
    public required int Page { get; set; } = 0;

    [DefaultValue(100)]
    public required int PageSize { get; set; } = 100;
}
