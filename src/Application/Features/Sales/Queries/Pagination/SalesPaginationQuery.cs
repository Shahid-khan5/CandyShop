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
        return $"Listview:{ListView}, Search:{Keyword}, {OrderBy}, {SortDirection}, {PageNumber}, {PageSize}";
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
           var data = await _context.Sales.OrderBy($"{request.OrderBy} {request.SortDirection}")
                                    .ProjectToPaginatedDataAsync<Sale, SaleDto>(request.Specification, request.PageNumber, request.PageSize, _mapper.ConfigurationProvider, cancellationToken);
            return data;
        }
}