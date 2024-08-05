namespace CleanArchitecture.Blazor.Application.Features.Campaigns.Specifications;
#nullable disable warnings
/// <summary>
/// Specification class for filtering Campaigns by their ID.
/// </summary>
public class CampaignByIdSpecification : Specification<Campaign>
{
    public CampaignByIdSpecification(int id)
    {
       Query.Where(q => q.Id == id);
    }
}