using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ardalis.Specification.EntityFrameworkCore;
using CleanArchitecture.Blazor.Application.Features.Sales.Caching;
using CleanArchitecture.Blazor.Application.Features.Sales.DTOs;
using CleanArchitecture.Blazor.Application.Features.Sales.Specifications;
using CleanArchitecture.Blazor.Domain.Entities;
using DocumentFormat.OpenXml.Wordprocessing;
using MailKit.Search;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;

namespace CleanArchitecture.Blazor.Application.Features.Sales.Queries.GetSalesSummaryQuery;
//public class GetSalesSummaryQuery : IRequest<List<SaleSummaryDto>>
public class GetSalesSummaryQuery : ICacheableRequest<List<SaleSummaryDto>>
{
    public int? CampaignId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? UserId { get; set; }
    public bool? OnlyEndedCampaigns { get; set; }
    public override string ToString()
    {
        return $"CampaignId:{CampaignId}, StartDate:{StartDate}, EndDate:{EndDate}, UserId:{UserId}, OnlyEndedCampaigns:{OnlyEndedCampaigns}";
    }
    public string CacheKey => SaleCacheKey.GetSalesSummaryCacheKey($"{this}");
    public MemoryCacheEntryOptions? Options => SaleCacheKey.MemoryCacheEntryOptions;

}

public class GetSalesSummaryQueryHandler : IRequestHandler<GetSalesSummaryQuery, List<SaleSummaryDto>>
{
    private readonly IApplicationDbContext _context;

    public GetSalesSummaryQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<SaleSummaryDto>> Handle(GetSalesSummaryQuery request, CancellationToken cancellationToken)
    {
        var specification = new CampaignSummarySpecification(request);
        var query = await _context.Campaigns
            .WithSpecification(specification)
            .AsNoTracking()
            .ToListAsync();

        return FetchSalesData(query, cancellationToken);
    }

    private List<SaleSummaryDto> FetchSalesData(List<Campaign> query, CancellationToken cancellationToken)
    {

        return query
            .Select(s => new SaleSummaryDto
            {
                CampaignId = s.Id,
                ClassName = s.Name,
                AdminName = s.CampaignUsers.FirstOrDefault()?.User?.UserName ?? "",
                NumberOfOrders = s.Sales.Count,
                TotalAmount = s.Sales.Sum(s => s.TotalAmount),
                TotalCommission = s.Sales.Sum(s => s.SaleItems.Sum(si => si.TotalPrice - (si.Product.CostPrice * si.Quantity))),
                TotalCandiesSold = s.Sales.Sum(s => s.SaleItems.Sum(si => si.Quantity)),
                TotalCustomers = s.Sales.Select(s => s.CustomerEmail).Distinct().Count()
            })
            .ToList();
    }

    private List<SaleSummaryDto> AggregateSalesData(List<SaleData> salesData)
    {
        return salesData
            .GroupBy(s => new { s.CampaignId, s.CampaignName, s.UserName })
            .Select(g => new SaleSummaryDto
            {
                CampaignId = g.Key.CampaignId,
                ClassName = g.Key.CampaignName,
                AdminName = g.Key.UserName,
                TotalCandiesSold = g.Sum(s => s.SaleItems.Sum(si => si.Quantity)),
                TotalCommission = g.Sum(s => s.SaleItems.Sum(si => si.TotalPrice) - s.SaleItems.Sum(si => si.UnitPrice * si.Quantity)),
                TotalAmount = g.Sum(s => s.TotalAmount),
                NumberOfOrders = g.Count(),
                TotalCustomers = g.Select(s => s.CustomerEmail).Distinct().Count()
            })
            .ToList();
    }
}
