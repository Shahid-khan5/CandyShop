using CleanArchitecture.Blazor.Application.Features.Dashboard.Caching;

namespace CleanArchitecture.Blazor.Application.Features.Dashboard.Queries.GetDashboardTotal;

public record GetDashboardTotalsQuery : ICacheableRequest<DashboardTotalsDto>
{
    public int CampaignId { get; set; }
    public MemoryCacheEntryOptions? Options => DashboardCacheKey.MemoryCacheEntryOptions;
    public string CacheKey => DashboardCacheKey.DashboardTotalsCacheKey;

}

public class GetDashboardTotalsQueryHandler : IRequestHandler<GetDashboardTotalsQuery, DashboardTotalsDto>
{
    private readonly IApplicationDbContext _context;

    public GetDashboardTotalsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardTotalsDto> Handle(GetDashboardTotalsQuery request, CancellationToken cancellationToken)
    {
        var totalSales = await _context.Sales.Where(x => x.CampaignId == request.CampaignId)
            .SumAsync(x => x.TotalAmount, cancellationToken);
        var totalCompletedOrders = await _context.Sales
            .Where(x => x.CampaignId == request.CampaignId)
            .CountAsync(cancellationToken);
        var totalCommission = totalSales * 0.1m;

        return new DashboardTotalsDto
        {
            TotalSales = totalSales,
            TotalCompletedOrders = totalCompletedOrders,
            TotalCommission = totalCommission
        };
    }
}

public class DashboardTotalsDto
{
    public decimal TotalSales { get; set; }
    public decimal TotalCommission { get; set; }
    public int TotalCompletedOrders { get; set; }
}