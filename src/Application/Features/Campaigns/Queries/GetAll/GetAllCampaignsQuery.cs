// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using CleanArchitecture.Blazor.Application.Features.Campaigns.DTOs;
using CleanArchitecture.Blazor.Application.Features.Campaigns.Caching;

namespace CleanArchitecture.Blazor.Application.Features.Campaigns.Queries.GetAll;

public class GetAllCampaignsQuery : ICacheableRequest<IEnumerable<CampaignDto>>
{
   public string CacheKey => CampaignCacheKey.GetAllCacheKey;
   public MemoryCacheEntryOptions? Options => CampaignCacheKey.MemoryCacheEntryOptions;
}

public class GetAllCampaignsQueryHandler :
     IRequestHandler<GetAllCampaignsQuery, IEnumerable<CampaignDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IStringLocalizer<GetAllCampaignsQueryHandler> _localizer;

    public GetAllCampaignsQueryHandler(
        IApplicationDbContext context,
        IMapper mapper,
        IStringLocalizer<GetAllCampaignsQueryHandler> localizer
        )
    {
        _context = context;
        _mapper = mapper;
        _localizer = localizer;
    }

    public async Task<IEnumerable<CampaignDto>> Handle(GetAllCampaignsQuery request, CancellationToken cancellationToken)
    {
        var data = await _context.Campaigns
                     .ProjectTo<CampaignDto>(_mapper.ConfigurationProvider)
                     .AsNoTracking()
                     .ToListAsync(cancellationToken);
        return data;
    }
}


