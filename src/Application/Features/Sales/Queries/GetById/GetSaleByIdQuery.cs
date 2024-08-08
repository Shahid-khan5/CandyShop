// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using CleanArchitecture.Blazor.Application.Features.Sales.DTOs;
using CleanArchitecture.Blazor.Application.Features.Sales.Caching;
using CleanArchitecture.Blazor.Application.Features.Sales.Specifications;

namespace CleanArchitecture.Blazor.Application.Features.Sales.Queries.GetById;

public class GetSaleByIdQuery : ICacheableRequest<SaleDto>
{
   public required int Id { get; set; }
   public string CacheKey => SaleCacheKey.GetByIdCacheKey($"{Id}");
   public MemoryCacheEntryOptions? Options => SaleCacheKey.MemoryCacheEntryOptions;
}

public class GetSaleByIdQueryHandler :
     IRequestHandler<GetSaleByIdQuery, SaleDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IStringLocalizer<GetSaleByIdQueryHandler> _localizer;

    public GetSaleByIdQueryHandler(
        IApplicationDbContext context,
        IMapper mapper,
        IStringLocalizer<GetSaleByIdQueryHandler> localizer
        )
    {
        _context = context;
        _mapper = mapper;
        _localizer = localizer;
    }

    public async Task<SaleDto> Handle(GetSaleByIdQuery request, CancellationToken cancellationToken)
    {
        var data = await _context.Sales.ApplySpecification(new SaleByIdSpecification(request.Id))
                     .ProjectTo<SaleDto>(_mapper.ConfigurationProvider)
                     .FirstAsync(cancellationToken) ?? throw new NotFoundException($"Sale with id: [{request.Id}] not found.");
        return data;
    }
}
