// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using CleanArchitecture.Blazor.Application.Features.Campaigns.DTOs;
using CleanArchitecture.Blazor.Application.Features.Campaigns.Caching;
using CleanArchitecture.Blazor.Application.Features.Campaigns.Specifications;

namespace CleanArchitecture.Blazor.Application.Features.Campaigns.Queries.GetById;

public class GetCampaignByIdQuery : ICacheableRequest<CampaignDto>
{
   public required int Id { get; set; }
   public string CacheKey => CampaignCacheKey.GetByIdCacheKey($"{Id}");
   public MemoryCacheEntryOptions? Options => CampaignCacheKey.MemoryCacheEntryOptions;
}

public class GetCampaignByIdQueryHandler :
     IRequestHandler<GetCampaignByIdQuery, CampaignDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IStringLocalizer<GetCampaignByIdQueryHandler> _localizer;

    public GetCampaignByIdQueryHandler(
        IApplicationDbContext context,
        IMapper mapper,
        IStringLocalizer<GetCampaignByIdQueryHandler> localizer
        )
    {
        _context = context;
        _mapper = mapper;
        _localizer = localizer;
    }

    public async Task<CampaignDto> Handle(GetCampaignByIdQuery request, CancellationToken cancellationToken)
    {
        var data = await _context.Campaigns.ApplySpecification(new CampaignByIdSpecification(request.Id))
                     .ProjectTo<CampaignDto>(_mapper.ConfigurationProvider)
                     .FirstAsync(cancellationToken) ?? throw new NotFoundException($"Campaign with id: [{request.Id}] not found.");
        return data;
    }
}
