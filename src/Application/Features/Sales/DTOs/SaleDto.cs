// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace CleanArchitecture.Blazor.Application.Features.Sales.DTOs;

[Description("Sales")]
public class SaleDto
{
    [Description("Id")]
    public int Id { get; set; }
    [Description("Campaign Id")]
    public int CampaignId {get;set;} 
    [Description("User Id")]
    public string? UserId {get;set;}
    [Description("StudentName")]
    public string? StudentName { get; set; }
    [Description("Customer Name")]
    public string? CustomerName {get;set;} 
    [Description("Customer Email")]
    public string? CustomerEmail {get;set;} 
    [Description("Total Amount")]
    public decimal TotalAmount {get;set;} 
    [Description("Sale Date")]
    public DateTime SaleDate {get;set;} 
    [Description("Created")]
    public DateTime Created {get;set;} 
    [Description("Created By")]
    public string? CreatedBy {get;set;} 
    [Description("Last Modified")]
    public DateTime? LastModified {get;set;} 
    [Description("Last Modified By")]
    public string? LastModifiedBy {get;set;} 


    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Sale, SaleDto>()
                .ForMember(x => x.StudentName, opts => opts.MapFrom(x => x.User.UserName))
                .ReverseMap();
        }
    }
}

