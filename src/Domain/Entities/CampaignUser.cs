// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations;
using CleanArchitecture.Blazor.Domain.Identity;

namespace CleanArchitecture.Blazor.Domain.Entities;

public class CampaignUser
{
    [MaxLength(450)]
    public string UserId { get; set; }

    [MaxLength(450)]
    public int CampaignId { get; set; }

    // Navigation properties
    public ApplicationUser User { get; set; }
    public Campaign Campaign { get; set; }
}
