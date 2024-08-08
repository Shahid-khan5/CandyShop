using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CleanArchitecture.Blazor.Application.Common.Interfaces;
using CleanArchitecture.Blazor.Application.Features.Dashboard.Caching;
using CleanArchitecture.Blazor.Application.Features.Dashboard.DTOs;

namespace CleanArchitecture.Blazor.Application.Features.Dashboard.Queries.GetStudentPerformanceQueryHandler;
public class GetStudentPerformanceQuery : ICacheableRequest<List<StudentPerformanceDto>>
{
    public int Month { get; set; }
    public int Year { get; set; }
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
        var sales = await GetSalesForPeriod(request.Year, request.Month, cancellationToken);
        var studentPerformance = CalculateStudentPerformance(sales);
        return studentPerformance;
    }

    private async Task<List<Sale>> GetSalesForPeriod(int year, int month, CancellationToken cancellationToken)
    {
        return await _context.Sales
            .Where(s => s.SaleDate.Year >= year && s.SaleDate.Month <= month)
            .Include(s => s.User)
            .Include(s => s.SaleItems)
            .ThenInclude(si => si.Product)
            .ToListAsync(cancellationToken);
    }

    private List<StudentPerformanceDto> CalculateStudentPerformance(List<Sale> sales)
    {
        return sales.GroupBy(s => new { s.User.UserName, s.SaleDate.Month })
            .Select(g => new StudentPerformanceDto
            {
                StudentName = g.Key.UserName ?? "",
                SalesAmount = g.Sum(s => s.TotalAmount),
                ProductSold = g.Sum(s => s.SaleItems.Sum(si => si.Quantity)),
                Commission = g.Sum(s => s.TotalAmount * 0.1m),
                Performance = g.Sum(s => s.TotalAmount) >= 1000 ? "Good" : "Bad"
            })
            .OrderByDescending(sp => sp.SalesAmount)
            .Take(5)
            .ToList();
    }
}