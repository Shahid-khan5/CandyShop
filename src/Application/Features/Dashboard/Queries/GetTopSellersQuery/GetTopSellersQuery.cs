using Ardalis.Specification.EntityFrameworkCore;
using CleanArchitecture.Blazor.Application.Features.Dashboard.Caching;
using CleanArchitecture.Blazor.Application.Features.Dashboard.Queries.GetCampaignRevenueQuery;

namespace CleanArchitecture.Blazor.Application.Features.Dashboard.Queries;

public class DashboardSpecification<T> : Specification<T> where T : class
{
    public DashboardSpecification(CampaignFilter filter)
    {
        Query.Where(GetCampaignPredicate(filter));
    }

    private static Expression<Func<T, bool>> GetCampaignPredicate(CampaignFilter filter)
    {
        return filter.CampaignId.HasValue
            ? x => EF.Property<int>(x, "CampaignId") == filter.CampaignId.Value
            : x => true;
    }
}

public class GetTop4SellersQuery : CampaignFilter, ICacheableRequest<IEnumerable<TopSellerDto>>
{
    public MemoryCacheEntryOptions? Options => DashboardCacheKey.MemoryCacheEntryOptions;
    public DashboardSpecification<Sale> Specification => new(this);
    public string CacheKey => DashboardCacheKey.Top4SellersKey(CampaignId);
}

public class GetTop4SellersQueryHandler : IRequestHandler<GetTop4SellersQuery, IEnumerable<TopSellerDto>>
{
    private readonly IApplicationDbContext _context;

    public GetTop4SellersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<TopSellerDto>> Handle(GetTop4SellersQuery request, CancellationToken cancellationToken)
    {
        var topSellers =await _context.Sales
            .WithSpecification(request.Specification)
            .GroupBy(s => s.UserId)
            .Select(g => new TopSellerDto
            {
                UserId = g.Key,
                Name = g.First().User.DisplayName,
                TotalSales = g.Sum(s => s.TotalAmount),
                TotalCommission = g.Sum(s => s.TotalAmount) * 0.1m // Assuming 10% commission
            })
            .OrderByDescending(s => s.TotalSales)
            .Take(5)
            .ToListAsync(cancellationToken);

        // Assign ranks
        for (int i = 0; i < topSellers.Count; i++)
        {
            topSellers[i].Rank = i + 1;
        }

        return topSellers;
    }
}

public class GetTop4StudentsInCampaignQuery : CampaignFilter, ICacheableRequest<IEnumerable<TopSellerDto>>
{
    public string? AdminId { get; set; }
    public MemoryCacheEntryOptions? Options => DashboardCacheKey.MemoryCacheEntryOptions;
    public DashboardSpecification<Sale> Specification => new(this);
    public string CacheKey => DashboardCacheKey.Top4StudentsKey(AdminId, CampaignId);

    public GetTop4StudentsInCampaignQuery(string adminId, int? campaignId = null)
    {
        AdminId = adminId;
        CampaignId = campaignId;
    }
}

public class GetTop4StudentsInCampaignQueryHandler : IRequestHandler<GetTop4StudentsInCampaignQuery, IEnumerable<TopSellerDto>>
{
    private readonly IApplicationDbContext _context;

    public GetTop4StudentsInCampaignQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<TopSellerDto>> Handle(GetTop4StudentsInCampaignQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Sales.WithSpecification(request.Specification);

        var adminCampaigns = await _context.CampaignUsers
            .Where(cu => cu.UserId == request.AdminId)
            .Select(cu => cu.CampaignId)
            .ToListAsync(cancellationToken);

        var topStudents = await query
            .Where(s => adminCampaigns.Contains(s.CampaignId))
            .GroupBy(s => s.UserId)
            .Select(g => new TopSellerDto
            {
                UserId = g.Key,
                Name = g.First().User.DisplayName,
                TotalSales = g.Sum(s => s.TotalAmount),
                TotalCommission = g.Sum(s => s.TotalAmount) * 0.1m // Assuming 10% commission
            })
            .OrderByDescending(s => s.TotalSales)
            .Take(5)
            .ToListAsync(cancellationToken);

        // Assign ranks
        for (int i = 0; i < topStudents.Count; i++)
        {
            topStudents[i].Rank = i + 1;
        }

        return topStudents;
    }
}

public class GetTop4ProductsQuery : CampaignFilter, ICacheableRequest<IEnumerable<TopProductDto>>
{
    public int? StudentId { get; set; }
    public MemoryCacheEntryOptions? Options => DashboardCacheKey.MemoryCacheEntryOptions;
    public DashboardSpecification<SaleItem> Specification => new(this);
    public string CacheKey => DashboardCacheKey.Top4ProductsKey(CampaignId);
}

public class GetTop4ProductsQueryHandler : IRequestHandler<GetTop4ProductsQuery, IEnumerable<TopProductDto>>
{
    private readonly IApplicationDbContext _context;

    public GetTop4ProductsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<TopProductDto>> Handle(GetTop4ProductsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.SaleItems
            .WithSpecification(request.Specification);
        var topProducts = await query
            .GroupBy(si => si.ProductId)
            .Select(g => new TopProductDto
            {
                ProductId = g.Key,
                Name = g.First().Product.Name,
                TotalSales = g.Sum(si => si.TotalPrice),
                QuantitySold = g.Sum(si => si.Quantity)
            })
            .OrderByDescending(p => p.TotalSales)
            .Take(4)
            .ToListAsync(cancellationToken);

        // Assign ranks
        for (int i = 0; i < topProducts.Count; i++)
        {
            topProducts[i].Rank = i + 1;
        }

        return topProducts;
    }
}



public class TopSellerDto
{
    public string UserId { get; set; }
    public string Name { get; set; }
    public decimal TotalSales { get; set; }
    public decimal TotalCommission { get; set; }
    public int Rank { get; set; }
}

public class TopProductDto
{
    public int ProductId { get; set; }
    public string Name { get; set; }
    public decimal TotalSales { get; set; }
    public int QuantitySold { get; set; }
    public int Rank { get; set; }
}