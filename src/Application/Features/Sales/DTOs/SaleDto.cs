// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace CleanArchitecture.Blazor.Application.Features.Sales.DTOs;

[Description("Sales")]
public class SaleDto
{
    [Description("Id")]
    public int Id { get; set; }
    [Description("Campaign Id")]
    public int CampaignId { get; set; }
    [Description("User Id")]
    public string? UserId { get; set; }
    [Description("Student Name")]
    public string? StudentName { get; set; }
    [Description("Class Name")]
    public string? ClassName { get; set; }
    [Description("Customer Name")]
    public string? CustomerName { get; set; }
    [Description("Customer Email")]
    public string? CustomerEmail { get; set; }
    [Description("Total Amount")]
    public decimal TotalAmount { get; set; }
    [Description("Total Sold")]
    public int TotalSold { get; set; }
    [Description("Sale Date")]
    public DateTime SaleDate { get; set; }
    [Description("Created")]
    public DateTime Created { get; set; }
    [Description("Created By")]
    public string? CreatedBy { get; set; }
    [Description("Last Modified")]
    public DateTime? LastModified { get; set; }
    [Description("Last Modified By")]
    public string? LastModifiedBy { get; set; }


    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Sale, SaleDto>()
                .ForMember(x => x.StudentName, opts => opts.MapFrom(x => x.User.UserName))
                .ForMember(x => x.ClassName, opts => opts.MapFrom(x => x.Campaign.Name))
                .ForMember(x => x.TotalSold, opts => opts.MapFrom(x => x.SaleItems.Count()))
                .ReverseMap();
        }
    }
}

public class SaleData
{
    public int CampaignId { get; set; }
    public string CampaignName { get; set; }
    public string UserName { get; set; }
    public decimal TotalAmount { get; set; }
    public string CustomerEmail { get; set; }
    public List<SaleItemData> SaleItems { get; set; }
}

public class SaleItemData
{
    public int Quantity { get; set; }
    public decimal TotalPrice { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal CostPrice { get; internal set; }
}