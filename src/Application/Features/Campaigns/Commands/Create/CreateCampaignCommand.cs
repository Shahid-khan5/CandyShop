// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System.ComponentModel;
using CleanArchitecture.Blazor.Application.Features.Campaigns.DTOs;
using CleanArchitecture.Blazor.Application.Features.Campaigns.Caching;

namespace CleanArchitecture.Blazor.Application.Features.Campaigns.Commands.Create;

public class CreateCampaignCommand: ICacheInvalidatorRequest<Result<int>>
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
             CreateMap<CampaignDto,CreateCampaignCommand>(MemberList.None);
             CreateMap<CreateCampaignCommand,Campaign>(MemberList.None);
        }
    }
}
    
    public class CreateCampaignCommandHandler : IRequestHandler<CreateCampaignCommand, Result<int>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IStringLocalizer<CreateCampaignCommand> _localizer;
        public CreateCampaignCommandHandler(
            IApplicationDbContext context,
            IStringLocalizer<CreateCampaignCommand> localizer,
            IMapper mapper
            )
        {
            _context = context;
            _localizer = localizer;
            _mapper = mapper;
        }
        public async Task<Result<int>> Handle(CreateCampaignCommand request, CancellationToken cancellationToken)
        {
           var item = _mapper.Map<Campaign>(request);
           // raise a create domain event
	       item.AddDomainEvent(new CampaignCreatedEvent(item));
           _context.Campaigns.Add(item);
           await _context.SaveChangesAsync(cancellationToken);
           return  await Result<int>.SuccessAsync(item.Id);
        }
    }

