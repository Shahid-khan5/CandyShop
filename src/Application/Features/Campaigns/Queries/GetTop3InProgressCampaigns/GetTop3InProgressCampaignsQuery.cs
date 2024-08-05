// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using CleanArchitecture.Blazor.Application.Features.Campaigns.DTOs;
using CleanArchitecture.Blazor.Application.Features.Campaigns.Caching;
using CleanArchitecture.Blazor.Domain.Entities;

namespace CleanArchitecture.Blazor.Application.Features.Campaigns.Queries.GetTop3InProgressCampaigns;
public class GetTop3InProgressCampaignsQuery : ICacheableRequest<IEnumerable<CampaignDto>>
{
    public string CacheKey => CampaignCacheKey.GetTop3InProgressCacheKey;
    public MemoryCacheEntryOptions? Options => CampaignCacheKey.MemoryCacheEntryOptions;
}

public class GetTop3InProgressCampaignsQueryHandler :
    IRequestHandler<GetTop3InProgressCampaignsQuery, IEnumerable<CampaignDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IStringLocalizer<GetTop3InProgressCampaignsQueryHandler> _localizer;

    public GetTop3InProgressCampaignsQueryHandler(
        IApplicationDbContext context,
        IMapper mapper,
        IStringLocalizer<GetTop3InProgressCampaignsQueryHandler> localizer
        )
    {
        _context = context;
        _mapper = mapper;
        _localizer = localizer;
    }

    public async Task<IEnumerable<CampaignDto>> Handle(GetTop3InProgressCampaignsQuery request, CancellationToken cancellationToken)
    {
        var currentDate = DateTime.UtcNow;
        var data = await _context.Campaigns
        .Where(c => c.EndDate > currentDate)
            .Where(c => c.Status == CampaignStatus.InProgress)
            .OrderBy(c => c.EndDate)
            .Take(3)
            .ProjectTo<CampaignDto>(_mapper.ConfigurationProvider)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
        return data;
    }
}