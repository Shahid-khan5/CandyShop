// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using CleanArchitecture.Blazor.Application.Features.Campaigns.DTOs;
using CleanArchitecture.Blazor.Application.Features.Campaigns.Caching;
namespace CleanArchitecture.Blazor.Application.Features.Campaigns.Commands.AddEdit;

public class AcceptCampaignCommand: ICacheInvalidatorRequest<Result<int>>
{
      [Description("Id")]
      public int Id { get; set; }
      public string CacheKey => CampaignCacheKey.GetAllCacheKey;
      public CancellationTokenSource? SharedExpiryTokenSource => CampaignCacheKey.GetOrCreateTokenSource();

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<CampaignDto, AcceptCampaignCommand>(MemberList.None);
            CreateMap<AcceptCampaignCommand, Campaign>(MemberList.None);
         
        }
    }
}

public class AcceptCampaignCommandHandler : IRequestHandler<AcceptCampaignCommand, Result<int>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IStringLocalizer<AddEditCampaignCommandHandler> _localizer;
    public AcceptCampaignCommandHandler(
        IApplicationDbContext context,
        IStringLocalizer<AddEditCampaignCommandHandler> localizer,
        IMapper mapper
        )
    {
        _context = context;
        _localizer = localizer;
        _mapper = mapper;
    }
    public async Task<Result<int>> Handle(AcceptCampaignCommand request, CancellationToken cancellationToken)
    {
        if (request.Id > 0)
        {
            var item = await _context.Campaigns.FindAsync(new object[] { request.Id }, cancellationToken) ?? throw new NotFoundException($"Campaign with id: [{request.Id}] not found.");
            item.Status = CampaignStatus.InProgress;
            // raise a update domain event
            item.AddDomainEvent(new CampaignUpdatedEvent(item));
            await _context.SaveChangesAsync(cancellationToken);
            return await Result<int>.SuccessAsync(item.Id);
        }
        throw new InvalidOperationException("Id cannot be 0");
    }
}

