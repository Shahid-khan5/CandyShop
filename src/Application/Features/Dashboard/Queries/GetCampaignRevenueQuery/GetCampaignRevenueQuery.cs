using Ardalis.Specification.EntityFrameworkCore;
using CleanArchitecture.Blazor.Application.Features.Dashboard.Caching;
using CleanArchitecture.Blazor.Application.Features.Dashboard.DTOs;
using CleanArchitecture.Blazor.Domain.Identity;
using Microsoft.AspNetCore.Identity;

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
        return await _context.Campaigns
            .GroupBy(c => c.Name)
            .Select(g => new CampaignRevenueDto
            {
                CampaignName = g.Key,
                TotalRevenue = g.SelectMany(c => c.Sales).Sum(s => s.TotalAmount),
                TotalCommission = g.SelectMany(c => c.Sales).Sum(s => s.TotalAmount * 0.1m)
            })
            .ToListAsync(cancellationToken);
    }
}





public class CampaignFilter
{
    public int? CampaignId { get; set; }
}

public class DashboardSuperAdminSpecification : Specification<Campaign>
{
    public DashboardSuperAdminSpecification(CampaignFilter filter)
    {
        Query
               .Where(x => x.Id == filter.CampaignId, filter.CampaignId > 0);
    }
}
public class GetCampaignRevenueAndProfitQuery : CampaignFilter, ICacheableRequest<List<ChartDataPoint>>
{
    public MemoryCacheEntryOptions? Options => DashboardCacheKey.MemoryCacheEntryOptions;
    public DashboardSuperAdminSpecification Specification => new(this);
    public string CacheKey => DashboardCacheKey.CampaignRevenueAndProfitKey(CampaignId);
}

public class GetCampaignRevenueAndProfitQueryHandler : IRequestHandler<GetCampaignRevenueAndProfitQuery, List<ChartDataPoint>>
{
    private readonly IApplicationDbContext _context;

    public GetCampaignRevenueAndProfitQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ChartDataPoint>> Handle(GetCampaignRevenueAndProfitQuery request, CancellationToken cancellationToken)
    {
        var campaignRevenue = await _context.Campaigns
        .WithSpecification(request.Specification)
        .GroupJoin(_context.Sales,
            c => c.Id,
            s => s.CampaignId,
            (campaign, sales) => new
            {
                CampaignId = campaign.Id,
                CampaignName = campaign.Name,
                Revenue = sales.Sum(s => s.TotalAmount)
            })
        .Take(10)
        .ToListAsync(cancellationToken);

        // Query 2: Get cost per campaign
        var campaignCost = await _context.Campaigns
            .GroupJoin(_context.Sales,
                c => c.Id,
                s => s.CampaignId,
                (campaign, sales) => new
                {
                    CampaignId = campaign.Id,
                    Cost = sales.SelectMany(s => s.SaleItems)
                        .Sum(si => si.Product.CostPrice * si.Quantity)
                })
            .ToListAsync(cancellationToken);

        // Combine results
        var result = campaignRevenue
            .GroupJoin(campaignCost,
                r => r.CampaignId,
                c => c.CampaignId,
                (r, c) => new ChartDataPoint
                {
                    Name = r.CampaignName,
                    Revenue = r.Revenue,
                    Profit = r.Revenue - (c.FirstOrDefault()?.Cost ?? 0)
                })
            .ToList();

        return result;
    }
}

public class GetStudentRevenueAndProfitQuery : IRequest<List<ChartDataPoint>>
{
    public string AdminId { get; set; }
}

public class GetStudentRevenueAndProfitQueryHandler : IRequestHandler<GetStudentRevenueAndProfitQuery, List<ChartDataPoint>>
{
    private readonly IApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public GetStudentRevenueAndProfitQueryHandler(IApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<List<ChartDataPoint>> Handle(GetStudentRevenueAndProfitQuery request, CancellationToken cancellationToken)
    {
        // Get the campaign for the admin
        var adminCampaign = await _context.Campaigns
            .Where(c => c.CampaignUsers.Any(cu => cu.UserId == request.AdminId))
            .Select(c => c.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (adminCampaign == 0) // No campaign found for the admin
        {
            return new List<ChartDataPoint>();
        }

        // Get all students in this campaign
        var studentsInCampaign = await _context.CampaignUsers
            .Where(cu => cu.CampaignId == adminCampaign)
            .Select(cu => new { cu.UserId, Name = (cu.User.DisplayName ?? cu.User.UserName ?? "No Name") })
            .ToListAsync(cancellationToken);

        // Calculate revenue and profit for each student
        var salesData = await _context.Sales
       .Where(s => studentsInCampaign.Select(std => std.UserId).Contains(s.UserId) && s.CampaignId == adminCampaign)
       .SelectMany(s => s.SaleItems.Select(si => new
       {
           s.UserId,
           Revenue = s.TotalAmount,
           Cost = si.Product.CostPrice * si.Quantity
       }))
       .GroupBy(s => s.UserId)
       .Select(g => new
       {
           UserId = g.Key,
           Revenue = g.Sum(s => s.Revenue),
           Cost = g.Sum(s => s.Cost)
       })
       .ToListAsync(cancellationToken);

        var result = salesData.Select(sd => new ChartDataPoint
        {
            Name = studentsInCampaign.First(x => x.UserId == sd.UserId).Name,
            Revenue = sd.Revenue,
            Profit = sd.Revenue - sd.Cost
        }).ToList();
        return result;
    }
}

public class ChartDataPoint
{
    public string Name { get; set; }
    public decimal Revenue { get; set; }
    public decimal Profit { get; set; }
}
