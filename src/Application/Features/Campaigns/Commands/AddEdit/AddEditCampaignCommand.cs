// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using CleanArchitecture.Blazor.Application.Features.Campaigns.DTOs;
using CleanArchitecture.Blazor.Application.Features.Campaigns.Caching;
namespace CleanArchitecture.Blazor.Application.Features.Campaigns.Commands.AddEdit;

public class AddEditCampaignCommand: ICacheInvalidatorRequest<Result<int>>
{
      [Description("Id")]
      public int Id { get; set; }
          [Description("Name")]
    public string Name {get;set;} = String.Empty; 
    [Description("Description")]
    public string? Description {get;set;} 
    [Description("Start Date")]
    public DateTime StartDate {get;set;} 
    [Description("End Date")]
    public DateTime EndDate {get;set;} 


      public string CacheKey => CampaignCacheKey.GetAllCacheKey;
      public CancellationTokenSource? SharedExpiryTokenSource => CampaignCacheKey.GetOrCreateTokenSource();

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<CampaignDto,AddEditCampaignCommand>(MemberList.None);
            CreateMap<AddEditCampaignCommand,Campaign>(MemberList.None);
         
        }
    }
}

    public class AddEditCampaignCommandHandler : IRequestHandler<AddEditCampaignCommand, Result<int>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IStringLocalizer<AddEditCampaignCommandHandler> _localizer;
        public AddEditCampaignCommandHandler(
            IApplicationDbContext context,
            IStringLocalizer<AddEditCampaignCommandHandler> localizer,
            IMapper mapper
            )
        {
            _context = context;
            _localizer = localizer;
            _mapper = mapper;
        }
        public async Task<Result<int>> Handle(AddEditCampaignCommand request, CancellationToken cancellationToken)
        {
            if (request.Id > 0)
            {
                var item = await _context.Campaigns.FindAsync(new object[] { request.Id }, cancellationToken) ?? throw new NotFoundException($"Campaign with id: [{request.Id}] not found.");
                item = _mapper.Map(request, item);
				// raise a update domain event
				item.AddDomainEvent(new CampaignUpdatedEvent(item));
                await _context.SaveChangesAsync(cancellationToken);
                return await Result<int>.SuccessAsync(item.Id);
            }
            else
            {
                var item = _mapper.Map<Campaign>(request);
                // raise a create domain event
				item.AddDomainEvent(new CampaignCreatedEvent(item));
                _context.Campaigns.Add(item);
                await _context.SaveChangesAsync(cancellationToken);
                return await Result<int>.SuccessAsync(item.Id);
            }
           
        }
    }

