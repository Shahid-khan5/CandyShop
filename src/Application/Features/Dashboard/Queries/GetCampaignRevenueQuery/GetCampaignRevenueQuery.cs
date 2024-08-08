using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CleanArchitecture.Blazor.Application.Features.Dashboard.Caching;
using CleanArchitecture.Blazor.Application.Features.Dashboard.DTOs;

namespace CleanArchitecture.Blazor.Application.Features.Dashboard.Queries.GetCampaignRevenueQuery;
public class GetCampaignRevenueQuery : ICacheableRequest<List<CampaignRevenueDto>>
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public MemoryCacheEntryOptions? Options => DashboardCacheKey.MemoryCacheEntryOptions;
    public string CacheKey => DashboardCacheKey.DashbaordCampaignRevenueCacheKey;
}

public class GetCampaignRevenueQueryHandler : IRequestHandler<GetCampaignRevenueQuery, List<CampaignRevenueDto>>
{
    private readonly IApplicationDbContext _context;

    public GetCampaignRevenueQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<CampaignRevenueDto>> Handle(GetCampaignRevenueQuery request, CancellationToken cancellationToken)
    {
        var result = await _context.Campaigns
            .GroupBy(c => c.Name)
            .Select(g => new CampaignRevenueDto
            {
                CampaignName = g.Key,
                TotalRevenue = g.SelectMany(c => c.Sales).Sum(s => s.TotalAmount),
                TotalCommission = g.SelectMany(c => c.Sales).Sum(s => s.TotalAmount * 0.1m)
             })
            .ToListAsync(cancellationToken);

        return result;
    }
}
