namespace CleanArchitecture.Blazor.Application.Features.Campaigns.Specifications;

#nullable disable warnings
/// <summary>
/// Specifies the different views available for the Campaign list.
/// </summary>
public enum CampaignListView
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
/// A class for applying advanced filtering options to Campaign lists.
/// </summary>
public class CampaignAdvancedFilter: PaginationFilter
{
    public CampaignListView ListView { get; set; } = CampaignListView.All;
    public UserProfile? CurrentUser { get; set; }
}