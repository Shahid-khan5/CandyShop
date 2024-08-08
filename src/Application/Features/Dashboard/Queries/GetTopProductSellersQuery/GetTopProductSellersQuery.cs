using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CleanArchitecture.Blazor.Application.Features.Dashboard.Caching;
using CleanArchitecture.Blazor.Application.Features.Dashboard.DTOs;

namespace CleanArchitecture.Blazor.Application.Features.Dashboard.Queries.GetTopProductSellersQuery;
public class GetTopProductSellersQuery : ICacheableRequest<List<TopProductSellerDto>>
{
    public int Month { get; set; }
    public int Year { get; set; }
    public MemoryCacheEntryOptions? Options => DashboardCacheKey.MemoryCacheEntryOptions;
    public string CacheKey => DashboardCacheKey.DashbaordTopProductSellersCacheKey;
}

public class GetTopProductSellersQueryHandler : IRequestHandler<GetTopProductSellersQuery, List<TopProductSellerDto>>
{
    private readonly IApplicationDbContext _context;

    public GetTopProductSellersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<TopProductSellerDto>> Handle(GetTopProductSellersQuery request, CancellationToken cancellationToken)
    {
        var sales = await GetSalesForPeriod(request.Year, request.Month, cancellationToken);
        return GetSalesForPeriod(sales);
    }

    private async Task<List<Sale>> GetSalesForPeriod(int year, int month, CancellationToken cancellationToken)
    {
        return await _context.Sales
            .Where(s => s.SaleDate.Year == year && s.SaleDate.Month == month)
            .Include(s => s.SaleItems)
            .ThenInclude(si => si.Product)
            .ToListAsync(cancellationToken);
    }

    private List<TopProductSellerDto> GetSalesForPeriod(List<Sale> sales)
    {
        return sales.SelectMany(s => s.SaleItems)
           .GroupBy(si => si.Product.Name)
           .Select(g => new TopProductSellerDto
           {
               ProductName = g.Key,
               ProductsSold = g.Sum(si => si.Quantity),
               TotalCapacity = 500
           })
           .OrderByDescending(tps => tps.ProductsSold)
           .Take(3)
           .ToList();
    }


}
