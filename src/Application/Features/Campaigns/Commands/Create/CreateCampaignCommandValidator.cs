// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace CleanArchitecture.Blazor.Application.Features.Campaigns.Commands.Create;

public class CreateCampaignCommandValidator : AbstractValidator<CreateCampaignCommand>
{
        public CreateCampaignCommandValidator()
        {
           
            RuleFor(v => v.Name)
                 .MaximumLength(256)
                 .NotEmpty();
        
        }
       
}

