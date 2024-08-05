// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using CleanArchitecture.Blazor.Application.Features.Campaigns.Caching;


namespace CleanArchitecture.Blazor.Application.Features.Campaigns.Commands.Delete;

    public class DeleteCampaignCommand:  ICacheInvalidatorRequest<Result<int>>
    {
      public int[] Id {  get; }
      public string CacheKey => CampaignCacheKey.GetAllCacheKey;
      public CancellationTokenSource? SharedExpiryTokenSource => CampaignCacheKey.GetOrCreateTokenSource();
      public DeleteCampaignCommand(int[] id)
      {
        Id = id;
      }
    }

    public class DeleteCampaignCommandHandler : 
                 IRequestHandler<DeleteCampaignCommand, Result<int>>

    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IStringLocalizer<DeleteCampaignCommandHandler> _localizer;
        public DeleteCampaignCommandHandler(
            IApplicationDbContext context,
            IStringLocalizer<DeleteCampaignCommandHandler> localizer,
             IMapper mapper
            )
        {
            _context = context;
            _localizer = localizer;
            _mapper = mapper;
        }
        public async Task<Result<int>> Handle(DeleteCampaignCommand request, CancellationToken cancellationToken)
        {
            var items = await _context.Campaigns.Where(x=>request.Id.Contains(x.Id)).ToListAsync(cancellationToken);
            foreach (var item in items)
            {
			    // raise a delete domain event
				item.AddDomainEvent(new CampaignDeletedEvent(item));
                _context.Campaigns.Remove(item);
            }
            var result = await _context.SaveChangesAsync(cancellationToken);
            return await Result<int>.SuccessAsync(result);
        }

    }

