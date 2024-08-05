// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using CleanArchitecture.Blazor.Application.Features.Campaigns.DTOs;
using CleanArchitecture.Blazor.Application.Features.Campaigns.Specifications;
using CleanArchitecture.Blazor.Application.Features.Campaigns.Queries.Pagination;

namespace CleanArchitecture.Blazor.Application.Features.Campaigns.Queries.Export;

public class ExportCampaignsQuery : CampaignAdvancedFilter, IRequest<Result<byte[]>>
{
      public CampaignAdvancedSpecification Specification => new CampaignAdvancedSpecification(this);
}
    
public class ExportCampaignsQueryHandler :
         IRequestHandler<ExportCampaignsQuery, Result<byte[]>>
{
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IExcelService _excelService;
        private readonly IStringLocalizer<ExportCampaignsQueryHandler> _localizer;
        private readonly CampaignDto _dto = new();
        public ExportCampaignsQueryHandler(
            IApplicationDbContext context,
            IMapper mapper,
            IExcelService excelService,
            IStringLocalizer<ExportCampaignsQueryHandler> localizer
            )
        {
            _context = context;
            _mapper = mapper;
            _excelService = excelService;
            _localizer = localizer;
        }
        #nullable disable warnings
        public async Task<Result<byte[]>> Handle(ExportCampaignsQuery request, CancellationToken cancellationToken)
        {
            var data = await _context.Campaigns.ApplySpecification(request.Specification)
                       .OrderBy($"{request.OrderBy} {request.SortDirection}")
                       .ProjectTo<CampaignDto>(_mapper.ConfigurationProvider)
                       .AsNoTracking()
                       .ToListAsync(cancellationToken);
            var result = await _excelService.ExportAsync(data,
                new Dictionary<string, Func<CampaignDto, object?>>()
                {
                    // TODO: Define the fields that should be exported, for example:
                    {_localizer[_dto.GetMemberDescription(x=>x.Name)],item => item.Name}, 
{_localizer[_dto.GetMemberDescription(x=>x.Description)],item => item.Description}, 
{_localizer[_dto.GetMemberDescription(x=>x.StartDate)],item => item.StartDate}, 
{_localizer[_dto.GetMemberDescription(x=>x.EndDate)],item => item.EndDate}, 

                }
                , _localizer[_dto.GetClassDescription()]);
            return await Result<byte[]>.SuccessAsync(result);
        }
}
