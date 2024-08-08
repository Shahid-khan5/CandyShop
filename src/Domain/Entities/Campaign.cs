// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations;
using CleanArchitecture.Blazor.Domain.Common.Entities;

namespace CleanArchitecture.Blazor.Domain.Entities;

public class Campaign : BaseEntity
{
    [Required]
    [MaxLength(450)]
    public required string Name { get; set; }
    public string StudentName { get; set; } = String.Empty;
    public string Email { get; set; }
    public string ContactNo { get; set; }
    public required string Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public CampaignStatus Status { get; set; }
    public required ICollection<CampaignUser> CampaignUsers { get; set; }
    public ICollection<Sale> Sales { get; set; }
    = new List<Sale>();
}
public enum CampaignStatus
{
    Requested,
    InProgress,
    Denied,
    Ended
}