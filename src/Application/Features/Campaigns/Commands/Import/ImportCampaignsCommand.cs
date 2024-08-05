// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using CleanArchitecture.Blazor.Application.Features.Campaigns.DTOs;
using CleanArchitecture.Blazor.Application.Features.Campaigns.Caching;

namespace CleanArchitecture.Blazor.Application.Features.Campaigns.Commands.Import;

    public class ImportCampaignsCommand: ICacheInvalidatorRequest<Result<int>>
    {
        public string FileName { get; set; }
        public byte[] Data { get; set; }
        public string CacheKey => CampaignCacheKey.GetAllCacheKey;
        public CancellationTokenSource? SharedExpiryTokenSource => CampaignCacheKey.GetOrCreateTokenSource();
        public ImportCampaignsCommand(string fileName,byte[] data)
        {
           FileName = fileName;
           Data = data;
        }
    }
    public record class CreateCampaignsTemplateCommand : IRequest<Result<byte[]>>
    {
 
    }

    public class ImportCampaignsCommandHandler : 
                 IRequestHandler<CreateCampaignsTemplateCommand, Result<byte[]>>,
                 IRequestHandler<ImportCampaignsCommand, Result<int>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IStringLocalizer<ImportCampaignsCommandHandler> _localizer;
        private readonly IExcelService _excelService;
        private readonly CampaignDto _dto = new();

        public ImportCampaignsCommandHandler(
            IApplicationDbContext context,
            IExcelService excelService,
            IStringLocalizer<ImportCampaignsCommandHandler> localizer,
            IMapper mapper
            )
        {
            _context = context;
            _localizer = localizer;
            _excelService = excelService;
            _mapper = mapper;
        }
        #nullable disable warnings
        public async Task<Result<int>> Handle(ImportCampaignsCommand request, CancellationToken cancellationToken)
        {

           var result = await _excelService.ImportAsync(request.Data, mappers: new Dictionary<string, Func<DataRow, CampaignDto, object?>>
            {
                { _localizer[_dto.GetMemberDescription(x=>x.Name)], (row, item) => item.Name = row[_localizer[_dto.GetMemberDescription(x=>x.Name)]].ToString() }, 
{ _localizer[_dto.GetMemberDescription(x=>x.Description)], (row, item) => item.Description = row[_localizer[_dto.GetMemberDescription(x=>x.Description)]].ToString() }, 
{ _localizer[_dto.GetMemberDescription(x=>x.StartDate)], (row, item) => item.StartDate =DateTime.Parse(row[_localizer[_dto.GetMemberDescription(x=>x.StartDate)]].ToString()) }, 
{ _localizer[_dto.GetMemberDescription(x=>x.EndDate)], (row, item) => item.EndDate = DateTime.Parse(row[_localizer[_dto.GetMemberDescription(x => x.EndDate)]].ToString()) }, 

            }, _localizer[_dto.GetClassDescription()]);
            if (result.Succeeded && result.Data is not null)
            {
                foreach (var dto in result.Data)
                {
                    var exists = await _context.Campaigns.AnyAsync(x => x.Name == dto.Name, cancellationToken);
                    if (!exists)
                    {
                        var item = _mapper.Map<Campaign>(dto);
                        // add create domain events if this entity implement the IHasDomainEvent interface
				        // item.AddDomainEvent(new CampaignCreatedEvent(item));
                        await _context.Campaigns.AddAsync(item, cancellationToken);
                    }
                 }
                 await _context.SaveChangesAsync(cancellationToken);
                 return await Result<int>.SuccessAsync(result.Data.Count());
           }
           else
           {
               return await Result<int>.FailureAsync(result.Errors);
           }
        }
        public async Task<Result<byte[]>> Handle(CreateCampaignsTemplateCommand request, CancellationToken cancellationToken)
        {
            // TODO: Implement ImportCampaignsCommandHandler method 
            var fields = new string[] {
                   // TODO: Define the fields that should be generate in the template, for example:
                   _localizer[_dto.GetMemberDescription(x=>x.Name)], 
_localizer[_dto.GetMemberDescription(x=>x.Description)], 
_localizer[_dto.GetMemberDescription(x=>x.StartDate)], 
_localizer[_dto.GetMemberDescription(x=>x.EndDate)], 

                };
            var result = await _excelService.CreateTemplateAsync(fields, _localizer[_dto.GetClassDescription()]);
            return await Result<byte[]>.SuccessAsync(result);
        }
    }

