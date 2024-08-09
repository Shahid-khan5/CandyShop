// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using CleanArchitecture.Blazor.Application.Features.Sales.DTOs;
using CleanArchitecture.Blazor.Application.Features.Sales.Caching;
using CleanArchitecture.Blazor.Application.Features.Sales.Specifications;

namespace CleanArchitecture.Blazor.Application.Features.Sales.Queries.Pagination;

public class SalesWithPaginationQuery : SaleAdvancedFilter, ICacheableRequest<PaginatedData<SaleDto>>
{
    public override string ToString()
    {
        return $"Listview:{ListView}, Search:{Keyword},UserId:{CurrentUser.UserId}, {OrderBy}, {SortDirection}, {PageNumber}, {PageSize}";
    }
    public string CacheKey => SaleCacheKey.GetPaginationCacheKey($"{this}");
    public MemoryCacheEntryOptions? Options => SaleCacheKey.MemoryCacheEntryOptions;
    public SaleAdvancedSpecification Specification => new SaleAdvancedSpecification(this);
}

public class SalesWithPaginationQueryHandler :
         IRequestHandler<SalesWithPaginationQuery, PaginatedData<SaleDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IStringLocalizer<SalesWithPaginationQueryHandler> _localizer;

    public SalesWithPaginationQueryHandler(
        IApplicationDbContext context,
        IMapper mapper,
        IStringLocalizer<SalesWithPaginationQueryHandler> localizer
        )
    {
        _context = context;
        _mapper = mapper;
        _localizer = localizer;
    }

    public async Task<PaginatedData<SaleDto>> Handle(SalesWithPaginationQuery request, CancellationToken cancellationToken)
    {
        var query = ApplyRoleBasedFilter(_context.Sales, request.CurrentUser);

        var data = await query
            .OrderBy($"{request.OrderBy} {request.SortDirection}")
            .ProjectToPaginatedDataAsync<Sale, SaleDto>(
                request.Specification,
                request.PageNumber,
                request.PageSize,
                _mapper.ConfigurationProvider,
                cancellationToken
            );
        return data;
    }

    private IQueryable<Sale> ApplyRoleBasedFilter(IQueryable<Sale> query, UserProfile currentUser)
    {
        return currentUser.DefaultRole switch
        {
            "SuperAdmin" => query,
            "Users" => query.Where(x => x.UserId == currentUser.UserId),
            "Admin" => query.Where(x => _context.CampaignUsers
            .Any(cu => cu.UserId == currentUser.UserId && cu.CampaignId == x.CampaignId)),
            _ => throw new ArgumentException("Invalid role")
        };
    }
}