namespace CleanArchitecture.Blazor.Application.Features.Sales.Specifications;

#nullable disable warnings
/// <summary>
/// Specifies the different views available for the Sale list.
/// </summary>
public enum SaleListView
{
    [Description("All")]
    All,
    [Description("My")]
    My,
    [Description("Created Toady")]
    CreatedToday,
    [Description("Created within the last 30 days")]
    Created30Days
}
/// <summary>
/// A class for applying advanced filtering options to Sale lists.
/// </summary>
public class SaleAdvancedFilter: PaginationFilter
{
    public SaleListView ListView { get; set; } = SaleListView.All;
    public UserProfile? CurrentUser { get; set; }
}