﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace CleanArchitecture.Blazor.Application.Features.Campaigns.Commands.AddEdit;

public class AddEditCampaignCommandValidator : AbstractValidator<AddEditCampaignCommand>
{
    public AddEditCampaignCommandValidator()
    {
            RuleFor(v => v.Name)
                .MaximumLength(256)
                .NotEmpty();
       
     }

}

