// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using CleanArchitecture.Blazor.Application.Features.Sales.DTOs;
using CleanArchitecture.Blazor.Application.Features.Sales.Caching;

namespace CleanArchitecture.Blazor.Application.Features.Sales.Commands.Import;

public class ImportSalesCommand : ICacheInvalidatorRequest<Result<int>>
{
    public string FileName { get; set; }
    public byte[] Data { get; set; }
    public string CacheKey => SaleCacheKey.GetAllCacheKey;
    public CancellationTokenSource? SharedExpiryTokenSource => SaleCacheKey.GetOrCreateTokenSource();
    public ImportSalesCommand(string fileName, byte[] data)
    {
        FileName = fileName;
        Data = data;
    }
}
public record class CreateSalesTemplateCommand : IRequest<Result<byte[]>>
{

}

public class ImportSalesCommandHandler :
             IRequestHandler<CreateSalesTemplateCommand, Result<byte[]>>,
             IRequestHandler<ImportSalesCommand, Result<int>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IStringLocalizer<ImportSalesCommandHandler> _localizer;
    private readonly IExcelService _excelService;
    private readonly SaleDto _dto = new();

    public ImportSalesCommandHandler(
        IApplicationDbContext context,
        IExcelService excelService,
        IStringLocalizer<ImportSalesCommandHandler> localizer,
        IMapper mapper
        )
    {
        _context = context;
        _localizer = localizer;
        _excelService = excelService;
        _mapper = mapper;
    }
#nullable disable warnings
    public async Task<Result<int>> Handle(ImportSalesCommand request, CancellationToken cancellationToken)
    {
        return null;
        //           var result = await _excelService.ImportAsync(request.Data, mappers: new Dictionary<string, Func<DataRow, SaleDto, object?>>
        //            {
        //                { _localizer[_dto.GetMemberDescription(x=>x.CampaignId)], (row, item) => item.CampaignId = row[_localizer[_dto.GetMemberDescription(x=>x.CampaignId)]].ToString() }, 
        //{ _localizer[_dto.GetMemberDescription(x=>x.UserId)], (row, item) => item.UserId = row[_localizer[_dto.GetMemberDescription(x=>x.UserId)]].ToString() }, 
        //{ _localizer[_dto.GetMemberDescription(x=>x.CustomerName)], (row, item) => item.CustomerName = row[_localizer[_dto.GetMemberDescription(x=>x.CustomerName)]].ToString() }, 
        //{ _localizer[_dto.GetMemberDescription(x=>x.CustomerEmail)], (row, item) => item.CustomerEmail = row[_localizer[_dto.GetMemberDescription(x=>x.CustomerEmail)]].ToString() }, 
        //{ _localizer[_dto.GetMemberDescription(x=>x.TotalAmount)], (row, item) => item.TotalAmount = row[_localizer[_dto.GetMemberDescription(x=>x.TotalAmount)]].ToString() }, 
        //{ _localizer[_dto.GetMemberDescription(x=>x.SaleDate)], (row, item) => item.SaleDate = row[_localizer[_dto.GetMemberDescription(x=>x.SaleDate)]].ToString() }, 
        //{ _localizer[_dto.GetMemberDescription(x=>x.Created)], (row, item) => item.Created = row[_localizer[_dto.GetMemberDescription(x=>x.Created)]].ToString() }, 
        //{ _localizer[_dto.GetMemberDescription(x=>x.CreatedBy)], (row, item) => item.CreatedBy = row[_localizer[_dto.GetMemberDescription(x=>x.CreatedBy)]].ToString() }, 
        //{ _localizer[_dto.GetMemberDescription(x=>x.LastModified)], (row, item) => item.LastModified = row[_localizer[_dto.GetMemberDescription(x=>x.LastModified)]].ToString() }, 
        //{ _localizer[_dto.GetMemberDescription(x=>x.LastModifiedBy)], (row, item) => item.LastModifiedBy = row[_localizer[_dto.GetMemberDescription(x=>x.LastModifiedBy)]].ToString() }, 

        //            }, _localizer[_dto.GetClassDescription()]);
        //            if (result.Succeeded && result.Data is not null)
        //            {
        //                foreach (var dto in result.Data)
        //                {
        //                    var exists = await _context.Sales.AnyAsync(x => x.Name == dto.Name, cancellationToken);
        //                    if (!exists)
        //                    {
        //                        var item = _mapper.Map<Sale>(dto);
        //                        // add create domain events if this entity implement the IHasDomainEvent interface
        //				        // item.AddDomainEvent(new SaleCreatedEvent(item));
        //                        await _context.Sales.AddAsync(item, cancellationToken);
        //                    }
        //                 }
        //                 await _context.SaveChangesAsync(cancellationToken);
        //                 return await Result<int>.SuccessAsync(result.Data.Count());
        //           }
        //           else
        //           {
        //               return await Result<int>.FailureAsync(result.Errors);
        //           }
    }
    public async Task<Result<byte[]>> Handle(CreateSalesTemplateCommand request, CancellationToken cancellationToken)
    {
        // TODO: Implement ImportSalesCommandHandler method 
        var fields = new string[] {
                   // TODO: Define the fields that should be generate in the template, for example:
                   _localizer[_dto.GetMemberDescription(x=>x.CampaignId)],
_localizer[_dto.GetMemberDescription(x=>x.UserId)],
_localizer[_dto.GetMemberDescription(x=>x.CustomerName)],
_localizer[_dto.GetMemberDescription(x=>x.CustomerEmail)],
_localizer[_dto.GetMemberDescription(x=>x.TotalAmount)],
_localizer[_dto.GetMemberDescription(x=>x.SaleDate)],
_localizer[_dto.GetMemberDescription(x=>x.Created)],
_localizer[_dto.GetMemberDescription(x=>x.CreatedBy)],
_localizer[_dto.GetMemberDescription(x=>x.LastModified)],
_localizer[_dto.GetMemberDescription(x=>x.LastModifiedBy)],

                };
        var result = await _excelService.CreateTemplateAsync(fields, _localizer[_dto.GetClassDescription()]);
        return await Result<byte[]>.SuccessAsync(result);
    }
}

