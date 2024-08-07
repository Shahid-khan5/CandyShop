// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using CleanArchitecture.Blazor.Application.Features.Sales.DTOs;
using CleanArchitecture.Blazor.Application.Features.Sales.Caching;

namespace CleanArchitecture.Blazor.Application.Features.Sales.Queries.GetAll;

public class GetAllSalesQuery : ICacheableRequest<IEnumerable<SaleDto>>
{
   public string CacheKey => SaleCacheKey.GetAllCacheKey;
   public MemoryCacheEntryOptions? Options => SaleCacheKey.MemoryCacheEntryOptions;
}

public class GetAllSalesQueryHandler :
     IRequestHandler<GetAllSalesQuery, IEnumerable<SaleDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IStringLocalizer<GetAllSalesQueryHandler> _localizer;

    public GetAllSalesQueryHandler(
        IApplicationDbContext context,
        IMapper mapper,
        IStringLocalizer<GetAllSalesQueryHandler> localizer
        )
    {
        _context = context;
        _mapper = mapper;
        _localizer = localizer;
    }

    public async Task<IEnumerable<SaleDto>> Handle(GetAllSalesQuery request, CancellationToken cancellationToken)
    {
        var data = await _context.Sales
                     .ProjectTo<SaleDto>(_mapper.ConfigurationProvider)
                     .AsNoTracking()
                     .ToListAsync(cancellationToken);
        return data;
    }
}


