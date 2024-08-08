using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CleanArchitecture.Blazor.Application.Features.Dashboard.Caching;
using CleanArchitecture.Blazor.Application.Features.Dashboard.DTOs;

namespace CleanArchitecture.Blazor.Application.Features.Dashboard.Queries;
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
        //var result = await _context.Campaigns
        //   .Where(c => c.Sales.Any(s =>
        //       (!request.StartDate.HasValue || s.SaleDate >= request.StartDate) &&
        //       (!request.EndDate.HasValue || s.SaleDate <= request.EndDate)))
        //   .Select(c => new CampaignRevenueDto
        //   {
        //       CampaignId = c.Id,
        //       CampaignName = c.Name,
        //       TotalRevenue = c.Sales
        //           .Where(s =>
        //               (!request.StartDate.HasValue || s.SaleDate >= request.StartDate) &&
        //               (!request.EndDate.HasValue || s.SaleDate <= request.EndDate))
        //           .Sum(s => s.TotalAmount),
        //       TotalCommission = c.Sales
        //           .Where(s =>
        //               (!request.StartDate.HasValue || s.SaleDate >= request.StartDate) &&
        //               (!request.EndDate.HasValue || s.SaleDate <= request.EndDate))
        //           .SelectMany(s => s.SaleItems)
        //           .Sum(si => si.TotalPrice - (si.Product.CostPrice * si.Quantity))
        //   })
        //   .OrderByDescending(c => c.TotalRevenue)
        //   .ToListAsync(cancellationToken);
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
