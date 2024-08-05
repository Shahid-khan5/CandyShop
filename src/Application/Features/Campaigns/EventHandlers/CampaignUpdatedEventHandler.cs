// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace CleanArchitecture.Blazor.Application.Features.Campaigns.EventHandlers;

    public class CampaignUpdatedEventHandler : INotificationHandler<CampaignUpdatedEvent>
    {
        private readonly ILogger<CampaignUpdatedEventHandler> _logger;

        public CampaignUpdatedEventHandler(
            ILogger<CampaignUpdatedEventHandler> logger
            )
        {
            _logger = logger;
        }
        public Task Handle(CampaignUpdatedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Domain Event: {DomainEvent}", notification.GetType().FullName);
            return Task.CompletedTask;
        }
    }
