// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace CleanArchitecture.Blazor.Application.Features.Campaigns.DTOs;

[Description("Campaigns")]
public class CampaignDto
{
    [Description("Id")]
    public int Id { get; set; }


    [Description("Email")]
    public string Email { get; set; }
    public string ContactNo { get; set; }



    [Description("Name")]
    public string Name {get;set;} = String.Empty; 
    public string StudentName {get;set;} = String.Empty; 
    [Description("Description")]
    public string? Description {get;set;} 
    [Description("Start Date")]
    public DateTime StartDate {get;set;} 
    [Description("End Date")]
    public DateTime EndDate {get;set;}

    [Description("Status")]
    public CampaignStatus Status { get; set; }
    public double TotalDays => (EndDate - StartDate).TotalDays;

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Campaign, CampaignDto>()
                .ReverseMap();
        }
    }
}

