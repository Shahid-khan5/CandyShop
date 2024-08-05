// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using CleanArchitecture.Blazor.Application.Features.Campaigns.DTOs;
using CleanArchitecture.Blazor.Application.Features.Campaigns.Caching;
using CleanArchitecture.Blazor.Application.Features.Campaigns.Specifications;

namespace CleanArchitecture.Blazor.Application.Features.Campaigns.Queries.Pagination;

public class CampaignsWithPaginationQuery : CampaignAdvancedFilter, ICacheableRequest<PaginatedData<CampaignDto>>
{
    public override string ToString()
    {
        return $"Listview:{ListView}, Search:{Keyword}, {OrderBy}, {SortDirection}, {PageNumber}, {PageSize}";
    }
    public string CacheKey => CampaignCacheKey.GetPaginationCacheKey($"{this}");
    public MemoryCacheEntryOptions? Options => CampaignCacheKey.MemoryCacheEntryOptions;
    public CampaignAdvancedSpecification Specification => new CampaignAdvancedSpecification(this);
}
    
public class CampaignsWithPaginationQueryHandler :
         IRequestHandler<CampaignsWithPaginationQuery, PaginatedData<CampaignDto>>
{
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IStringLocalizer<CampaignsWithPaginationQueryHandler> _localizer;

        public CampaignsWithPaginationQueryHandler(
            IApplicationDbContext context,
            IMapper mapper,
            IStringLocalizer<CampaignsWithPaginationQueryHandler> localizer
            )
        {
            _context = context;
            _mapper = mapper;
            _localizer = localizer;
        }

        public async Task<PaginatedData<CampaignDto>> Handle(CampaignsWithPaginationQuery request, CancellationToken cancellationToken)
        {
           var data = await _context.Campaigns.OrderBy($"{request.OrderBy} {request.SortDirection}")
                                    .ProjectToPaginatedDataAsync<Campaign, CampaignDto>(request.Specification, request.PageNumber, request.PageSize, _mapper.ConfigurationProvider, cancellationToken);
            return data;
        }
}