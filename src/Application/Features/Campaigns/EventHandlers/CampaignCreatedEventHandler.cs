// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace CleanArchitecture.Blazor.Application.Features.Campaigns.EventHandlers;

public class CampaignCreatedEventHandler : INotificationHandler<CampaignCreatedEvent>
{
        private readonly ILogger<CampaignCreatedEventHandler> _logger;

        public CampaignCreatedEventHandler(
            ILogger<CampaignCreatedEventHandler> logger
            )
        {
            _logger = logger;
        }
        public Task Handle(CampaignCreatedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Domain Event: {DomainEvent}", notification.GetType().FullName);
            return Task.CompletedTask;
        }
}
