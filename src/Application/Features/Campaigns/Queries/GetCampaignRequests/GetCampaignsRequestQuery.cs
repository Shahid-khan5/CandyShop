// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using CleanArchitecture.Blazor.Application.Features.Campaigns.DTOs;
using CleanArchitecture.Blazor.Application.Features.Campaigns.Caching;

namespace CleanArchitecture.Blazor.Application.Features.Campaigns.Queries.GetAll;

public class GetCampaignsRequestQuery : ICacheableRequest<IEnumerable<CampaignDto>>
{
   public string CacheKey => CampaignCacheKey.GetAllCacheKey;
   public MemoryCacheEntryOptions? Options => CampaignCacheKey.MemoryCacheEntryOptions;
}
public class GetCampaignsRequestQueryHandler :
     IRequestHandler<GetCampaignsRequestQuery, IEnumerable<CampaignDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IStringLocalizer<GetAllCampaignsQueryHandler> _localizer;

    public GetCampaignsRequestQueryHandler(
        IApplicationDbContext context,
        IMapper mapper,
        IStringLocalizer<GetAllCampaignsQueryHandler> localizer
        )
    {
        _context = context;
        _mapper = mapper;
        _localizer = localizer;
    }

    public async Task<IEnumerable<CampaignDto>> Handle(GetCampaignsRequestQuery request, CancellationToken cancellationToken)
    {
        var data = await _context.Campaigns
            .Where(x => x.Status == CampaignStatus.Requested)
                     .ProjectTo<CampaignDto>(_mapper.ConfigurationProvider)
                     .AsNoTracking()
                     .ToListAsync(cancellationToken);
        return data;
    }
}

