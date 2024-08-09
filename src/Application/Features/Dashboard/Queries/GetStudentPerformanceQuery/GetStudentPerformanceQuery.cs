using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ardalis.Specification.EntityFrameworkCore;
using CleanArchitecture.Blazor.Application.Common.Interfaces;
using CleanArchitecture.Blazor.Application.Features.Campaigns.Specifications;
using CleanArchitecture.Blazor.Application.Features.Dashboard.Caching;
using CleanArchitecture.Blazor.Application.Features.Dashboard.DTOs;
using CleanArchitecture.Blazor.Application.Features.Dashboard.Queries.GetCampaignRevenueQuery;

namespace CleanArchitecture.Blazor.Application.Features.Dashboard.Queries.GetStudentPerformanceQueryHandler;
public class GetStudentPerformanceQuery :CampaignFilter, ICacheableRequest<List<StudentPerformanceDto>>
{
    public DashboardSpecification<Sale> Specification => new(this);
    public MemoryCacheEntryOptions? Options => DashboardCacheKey.MemoryCacheEntryOptions;
    public string CacheKey => DashboardCacheKey.DashbaordStudentPerformanceCacheKey;
}


public class GetStudentPerformanceQueryHandler : IRequestHandler<GetStudentPerformanceQuery, List<StudentPerformanceDto>>
{
    private readonly IApplicationDbContext _context;

    public GetStudentPerformanceQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<StudentPerformanceDto>> Handle(GetStudentPerformanceQuery request, CancellationToken cancellationToken)
    {
        var sales = 

            await _context.Sales
            .WithSpecification(request.Specification)
            .Include(s => s.User)
            .Include(s => s.SaleItems)
            .ThenInclude(si => si.Product)
            .ToListAsync(cancellationToken);
        return CalculateStudentPerformance(sales);
    }

    private List<StudentPerformanceDto> CalculateStudentPerformance(List<Sale> sales)
    {
        return sales.GroupBy(s => new { s.User.UserName, s.SaleDate.Month })
            .Select(g => new StudentPerformanceDto
            {
                StudentName = g.Key.UserName ?? "",
                SalesAmount = g.Sum(s => s.TotalAmount),
                ProductSold = g.Sum(s => s.SaleItems.Sum(si => si.Quantity)),
                Commission = g.Sum(s => s.TotalAmount - (s.SaleItems.Sum(x=>x.Quantity * x.Product.CostPrice)) ),
                Performance = g.Sum(s => s.TotalAmount) >= 1000 ? "Good" : "Bad"
            })
            .OrderByDescending(sp => sp.SalesAmount)
            .ToList();
    }
}