// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using CleanArchitecture.Blazor.Application.Features.Sales.DTOs;
using CleanArchitecture.Blazor.Application.Features.Sales.Specifications;
using CleanArchitecture.Blazor.Application.Features.Sales.Queries.Pagination;

namespace CleanArchitecture.Blazor.Application.Features.Sales.Queries.Export;

public class ExportSalesQuery : SaleAdvancedFilter, IRequest<Result<byte[]>>
{
      public SaleAdvancedSpecification Specification => new SaleAdvancedSpecification(this);
}
    
public class ExportSalesQueryHandler :
         IRequestHandler<ExportSalesQuery, Result<byte[]>>
{
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IExcelService _excelService;
        private readonly IStringLocalizer<ExportSalesQueryHandler> _localizer;
        private readonly SaleDto _dto = new();
        public ExportSalesQueryHandler(
            IApplicationDbContext context,
            IMapper mapper,
            IExcelService excelService,
            IStringLocalizer<ExportSalesQueryHandler> localizer
            )
        {
            _context = context;
            _mapper = mapper;
            _excelService = excelService;
            _localizer = localizer;
        }
        #nullable disable warnings
        public async Task<Result<byte[]>> Handle(ExportSalesQuery request, CancellationToken cancellationToken)
        {
            var data = await _context.Sales.ApplySpecification(request.Specification)
                       .OrderBy($"{request.OrderBy} {request.SortDirection}")
                       .ProjectTo<SaleDto>(_mapper.ConfigurationProvider)
                       .AsNoTracking()
                       .ToListAsync(cancellationToken);
            var result = await _excelService.ExportAsync(data,
                new Dictionary<string, Func<SaleDto, object?>>()
                {
                    // TODO: Define the fields that should be exported, for example:
                    {_localizer[_dto.GetMemberDescription(x=>x.CampaignId)],item => item.CampaignId}, 
{_localizer[_dto.GetMemberDescription(x=>x.UserId)],item => item.UserId}, 
{_localizer[_dto.GetMemberDescription(x=>x.CustomerName)],item => item.CustomerName}, 
{_localizer[_dto.GetMemberDescription(x=>x.CustomerEmail)],item => item.CustomerEmail}, 
{_localizer[_dto.GetMemberDescription(x=>x.TotalAmount)],item => item.TotalAmount}, 
{_localizer[_dto.GetMemberDescription(x=>x.SaleDate)],item => item.SaleDate}, 
{_localizer[_dto.GetMemberDescription(x=>x.Created)],item => item.Created}, 
{_localizer[_dto.GetMemberDescription(x=>x.CreatedBy)],item => item.CreatedBy}, 
{_localizer[_dto.GetMemberDescription(x=>x.LastModified)],item => item.LastModified}, 
{_localizer[_dto.GetMemberDescription(x=>x.LastModifiedBy)],item => item.LastModifiedBy}, 

                }
                , _localizer[_dto.GetClassDescription()]);
            return await Result<byte[]>.SuccessAsync(result);
        }
}
