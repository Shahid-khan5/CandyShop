using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CleanArchitecture.Blazor.Application.Features.Sales.Queries.GetSalesSummaryQuery;

namespace CleanArchitecture.Blazor.Application.Features.Sales.Specifications;
public class SalesSummarySpecification : Specification<Sale>
{
    public SalesSummarySpecification(GetSalesSummaryQuery query)
    {
        if (query.CampaignId.HasValue)
        {
            Query.Where(s => s.CampaignId == query.CampaignId.Value);
        }

        if (query.StartDate.HasValue)
        {
            Query.Where(s => s.SaleDate >= query.StartDate.Value.Date);
        }

        if (query.EndDate.HasValue)
        {
            var endDate = query.EndDate.Value.Date.AddDays(1).AddTicks(-1);
            Query.Where(s => s.SaleDate <= endDate);
        }

        if (!string.IsNullOrEmpty(query.UserId))
        {
            Query.Where(s => s.UserId == query.UserId);
        }

        if (query.OnlyEndedCampaigns == true)
        {
            Query.Where(s => s.Campaign.Status == CampaignStatus.Ended);
        }

        Query.Include(s => s.Campaign)
             .Include(s => s.User)
             .Include(s => s.SaleItems)
                 .ThenInclude(si => si.Product);
    }
}
