// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanArchitecture.Blazor.Infrastructure.Persistence.Configurations;

public class CampaignUserConfiguration : IEntityTypeConfiguration<CampaignUser>
{
    public void Configure(EntityTypeBuilder<CampaignUser> builder)
    {
        builder.HasKey(cu => new { cu.UserId, cu.CampaignId });

        builder.HasOne(cu => cu.User)
            .WithMany(u => u.CampaignUsers)
            .HasForeignKey(cu => cu.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(cu => cu.Campaign)
            .WithMany(c => c.CampaignUsers)
            .HasForeignKey(cu => cu.CampaignId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
