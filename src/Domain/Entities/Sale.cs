// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using CleanArchitecture.Blazor.Domain.Identity;
using CleanArchitecture.Blazor.Domain.Common.Entities;

namespace CleanArchitecture.Blazor.Domain.Entities;

public class Sale : IEntity<int>
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(450)]
    public int CampaignId { get; set; }

    [Required]
    [MaxLength(450)]
    public string UserId { get; set; }

    [Required]
    [MaxLength(450)]
    public string CustomerName { get; set; }

    [Required]
    [MaxLength(450)]
    public string CustomerEmail { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }

    public DateTime SaleDate { get; set; }

    public DateTime Created { get; set; }

    [MaxLength(450)]
    public string CreatedBy { get; set; }

    public DateTime? LastModified { get; set; }

    [MaxLength(450)]
    public string LastModifiedBy { get; set; }

    // Navigation properties
    public Campaign Campaign { get; set; }
    public ApplicationUser User { get; set; }
    public ICollection<SaleItem> SaleItems { get; set; }
}
