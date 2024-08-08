using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CleanArchitecture.Blazor.Application.Features.Dashboard.Caching;
using CleanArchitecture.Blazor.Application.Features.Dashboard.DTOs;

namespace CleanArchitecture.Blazor.Application.Features.Dashboard.Queries.GetTopSellersQuery;
public class GetTopSellersQuery : ICacheableRequest<List<TopSellerDto>>
{
    public int Month { get; set; }
    public int Year { get; set; }
    public MemoryCacheEntryOptions? Options => DashboardCacheKey.MemoryCacheEntryOptions;
    public string CacheKey => DashboardCacheKey.DashbaordGetTopSellersCacheKey;
}

public class GetTopSellersQueryHandler : IRequestHandler<GetTopSellersQuery, List<TopSellerDto>>
{
    private readonly IApplicationDbContext _context;

    public GetTopSellersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<TopSellerDto>> Handle(GetTopSellersQuery request, CancellationToken cancellationToken)
    {
        var sales = await GetSalesForPeriod(request.Year, request.Month, cancellationToken);
        return CalculateTopSellers(sales);
    }

    private async Task<List<Sale>> GetSalesForPeriod(int year, int month, CancellationToken cancellationToken)
    {
        return await _context.Sales
            .Where(s => s.SaleDate.Year == year && s.SaleDate.Month == month)
            .Include(s => s.User)
            .ToListAsync(cancellationToken);
    }

    private List<TopSellerDto> CalculateTopSellers(List<Sale> sales)
    {
        return sales.GroupBy(s => s.User.DisplayName)
            .Select(g => new TopSellerDto
            {
                StudentName = g.Key,
                TotalSales = g.Sum(s => s.TotalAmount),
                TotalCommission = g.Sum(s => s.TotalAmount * 0.1m)
            })
            .OrderByDescending(ts => ts.TotalSales)
            .Take(5)
            .ToList();
    }


}
