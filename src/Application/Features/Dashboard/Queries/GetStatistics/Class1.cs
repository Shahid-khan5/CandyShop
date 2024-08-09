using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using CleanArchitecture.Blazor.Application.Features.Dashboard.Caching;

namespace CleanArchitecture.Blazor.Application.Features.Dashboard.Queries.GetStatistics;
public class DashboardStatisticsDto
{
    public decimal TotalSales { get; set; }
    public int TotalItems { get; set; }
    public decimal TotalCommission { get; set; }
    public int TotalOrders { get; set; }
}

public class GetDashboardStatisticsQuery : ICacheableRequest<DashboardStatisticsDto>
{
    public string UserId { get; set; }
    public string UserRole { get; set; }
    public int? CampaignId { get; set; }

    public MemoryCacheEntryOptions? Options => DashboardCacheKey.MemoryCacheEntryOptions;
    public string CacheKey => DashboardCacheKey.GetDashboardStatisticsKey(UserId, UserRole, CampaignId);
}

public class GetDashboardStatisticsQueryHandler : IRequestHandler<GetDashboardStatisticsQuery, DashboardStatisticsDto>
{
    private readonly IApplicationDbContext _context;

    public GetDashboardStatisticsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardStatisticsDto> Handle(GetDashboardStatisticsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Sales
            .Include(x=>x.SaleItems)
            .ThenInclude(x=>x.Product)
            .AsQueryable();

        switch (request.UserRole)
        {
            case "SuperAdmin" when request.CampaignId>0:
                query = query.Where(s => s.CampaignId==request.CampaignId);
                break;
            case "Admin":
                var adminCampaigns = await _context.CampaignUsers
                    .Where(cu => cu.UserId == request.UserId)
                    .Select(cu => cu.CampaignId)
                    .ToListAsync(cancellationToken);
                query = query.Where(s => adminCampaigns.Contains(s.CampaignId));
                break;
            case "Student":
                query = query.Where(s => s.UserId == request.UserId);
                if (request.CampaignId.HasValue)
                {
                    query = query.Where(s => s.CampaignId == request.CampaignId.Value);
                }
                break;
        }

        // Calculate the sum of SaleItems.Quantity for each sale first
        var salesWithItemQuantities =await query
            .Select(s => new
            {
                s.TotalAmount,
                TotalItems = s.SaleItems.Sum(si => si.Quantity),
                Commission = s.TotalAmount - s.SaleItems.Sum(si => si.Quantity * si.Product.CostPrice)
            }).ToListAsync();

        // Then perform the final aggregation
        var result = salesWithItemQuantities
            .GroupBy(s => 1)
            .Select(g => new DashboardStatisticsDto
            {
                TotalSales = g.Sum(s => s.TotalAmount),
                TotalItems = g.Sum(s => s.TotalItems),
                TotalCommission = g.Sum(s=>s.Commission),
                TotalOrders = g.Count()
            })
            .FirstOrDefault();

        return result ?? new DashboardStatisticsDto();
    }

}