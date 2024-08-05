// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace CleanArchitecture.Blazor.Application.Features.Campaigns.EventHandlers;

    public class CampaignDeletedEventHandler : INotificationHandler<CampaignDeletedEvent>
    {
        private readonly ILogger<CampaignDeletedEventHandler> _logger;

        public CampaignDeletedEventHandler(
            ILogger<CampaignDeletedEventHandler> logger
            )
        {
            _logger = logger;
        }
        public Task Handle(CampaignDeletedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Domain Event: {DomainEvent}", notification.GetType().FullName);
            return Task.CompletedTask;
        }
    }
