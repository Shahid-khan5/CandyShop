using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CleanArchitecture.Blazor.Application.Common.Mappings;
using CleanArchitecture.Blazor.Application.Features.Identity.Caching;
using CleanArchitecture.Blazor.Application.Features.Identity.DTOs;
using CleanArchitecture.Blazor.Application.Features.Products.Caching;
using CleanArchitecture.Blazor.Application.Features.Products.DTOs;
using CleanArchitecture.Blazor.Application.Features.Products.Specifications;
using CleanArchitecture.Blazor.Domain.Identity;

namespace CleanArchitecture.Blazor.Application.Features.Identity.Queries;
public class CampaignUsersWithPaginationQuery : PaginationFilter, ICacheableRequest<PaginatedData<ApplicationUserDto>>
{
    public string UserId { get; set; }
    public MemoryCacheEntryOptions? Options => ProductCacheKey.MemoryCacheEntryOptions;
    public string CacheKey => UserCacheKey.GetCampaignUsersCacheKey(UserId);

}
public class CampaignUsersWithPaginationQueryHandler :
    IRequestHandler<CampaignUsersWithPaginationQuery, PaginatedData<ApplicationUserDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public CampaignUsersWithPaginationQueryHandler(
        IApplicationDbContext context,
        IMapper mapper
    )
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PaginatedData<ApplicationUserDto>> Handle(CampaignUsersWithPaginationQuery request,
        CancellationToken cancellationToken)
    {
        var campaignId = await _context.CampaignUsers
            .Where(x => x.UserId == request.UserId)
            .Select(x => x.CampaignId)
            .FirstOrDefaultAsync(cancellationToken);

        var query = _context.CampaignUsers
               .Include(cu => cu.User)
               .Where(cu => cu.CampaignId == campaignId)
               .Select(cu => cu.User);

        var totalItems = await query.CountAsync(cancellationToken);

        var campaignUsers = await query
            .OrderBy($"{request.OrderBy} {request.SortDirection}")
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var dtos = _mapper.Map<List<ApplicationUserDto>>(campaignUsers);

        return new PaginatedData<ApplicationUserDto>(dtos, totalItems, request.PageNumber, request.PageSize);
    }

}

