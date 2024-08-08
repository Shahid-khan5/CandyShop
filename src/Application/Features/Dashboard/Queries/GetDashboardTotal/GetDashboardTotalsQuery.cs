using CleanArchitecture.Blazor.Application.Features.Dashboard.Caching;

namespace CleanArchitecture.Blazor.Application.Features.Dashboard.Queries.GetDashboardTotal;

public record GetDashboardTotalsQuery : ICacheableRequest<DashboardTotalsDto>
{
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
        var totalCampaigns = await _context.Campaigns.CountAsync(cancellationToken);
        var totalSales = await _context.Sales.SumAsync(x => x.TotalAmount, cancellationToken);
        var totalStudents = (await _context.Campaigns
            .Select(u => u.CampaignUsers.Count)
            .ToListAsync()).Sum();
        var totalCompletedOrders = await _context.Sales.CountAsync(cancellationToken);

        return new DashboardTotalsDto
        {
            TotalCampaigns = totalCampaigns,
            TotalSales = totalSales,
            TotalStudents = totalStudents,
            TotalCompletedOrders = totalCompletedOrders
        };
    }
}

public class DashboardTotalsDto
{
    public int TotalCampaigns { get; set; }
    public decimal TotalSales { get; set; }
    public int TotalStudents { get; set; }
    public int TotalCompletedOrders { get; set; }
}