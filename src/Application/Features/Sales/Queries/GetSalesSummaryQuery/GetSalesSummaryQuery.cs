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
public class GetSalesSummaryQuery : PaginationFilter, ICacheableRequest<PaginatedData<SaleSummaryDto>>
{
    public int? CampaignId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? UserId { get; set; }
    public bool? OnlyEndedCampaigns { get; set; }
    public override string ToString()
    {
        return $"CampaignId:{CampaignId}, StartDate:{StartDate}, EndDate:{EndDate}, UserId:{UserId},PageNumber:{PageNumber},PageSize:{PageSize},OrderBy:{OrderBy},SortDirection:{SortDirection},Keyword:{Keyword}";
    }
    public string CacheKey => SaleCacheKey.GetSalesSummaryCacheKey($"{this}");
    public MemoryCacheEntryOptions? Options => SaleCacheKey.MemoryCacheEntryOptions;

}

public class GetSalesSummaryQueryHandler : IRequestHandler<GetSalesSummaryQuery, PaginatedData<SaleSummaryDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetSalesSummaryQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PaginatedData<SaleSummaryDto>> Handle(GetSalesSummaryQuery request, CancellationToken cancellationToken)
    {
        //var specification = new CampaignSummarySpecification(request);
        //var query = await _context.Campaigns
        //    .WithSpecification(specification)
        //    .AsNoTracking()
        //    .ToListAsync();

        //return FetchSalesData(query, cancellationToken);
        var specification = new CampaignSummarySpecification(request);

        var query = _context.Campaigns
            .WithSpecification(specification)
            .AsNoTracking();

        var totalCount = await query.CountAsync(cancellationToken);

        var paginatedCampaigns = await query
            .OrderBy($"{request.OrderBy} {request.SortDirection}")
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var saleSummaries = FetchSalesData(paginatedCampaigns, cancellationToken);

        return new PaginatedData<SaleSummaryDto>(saleSummaries, totalCount, request.PageNumber, request.PageSize);
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

}
