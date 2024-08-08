using CleanArchitecture.Blazor.Application.Features.Campaigns.DTOs;
using MediatR;
using Microsoft.Extensions.Options;

namespace CleanArchitecture.Blazor.Application.Features.Identity.Notifications.UserActivation;

public record UserActivationNotification(string ActivationUrl, string Email, string UserId, string UserName)
    : INotification;

public class UserActivationNotificationHandler : INotificationHandler<UserActivationNotification>
{
    private readonly IStringLocalizer<AcceptCampaignNotification> _localizer;
    private readonly ILogger<AcceptCampaignNotification> _logger;
    private readonly IMailService _mailService;
    private readonly IApplicationSettings _settings;

    public UserActivationNotificationHandler(
        ILogger<AcceptCampaignNotification> logger,
        IStringLocalizer<AcceptCampaignNotification> localizer,
        IMailService mailService,
        IApplicationSettings settings)
    {
        _logger = logger;
        _localizer = localizer;
        _mailService = mailService;
        _settings = settings;
    }


    public async Task Handle(UserActivationNotification notification, CancellationToken cancellationToken)
    {
        var sendMailResult = await _mailService.SendAsync(
            notification.Email,
            _localizer["Account Activation Required"],
            "_useractivation",
            new
            {
                notification.ActivationUrl,
                _settings.AppName,
                _settings.Company,
                notification.UserName,
                notification.Email
            });
        _logger.LogInformation("Activation email sent to {Email}. sending result {Successful} {Message}",
            notification.Email, sendMailResult.Successful, string.Join(' ', sendMailResult.ErrorMessages));
    }
}



public record AcceptCampaignNotification(string Email,string Password,string LoginUrl, string Name)
    : INotification;

public class AcceptCampaignNotificationHandler : INotificationHandler<AcceptCampaignNotification>
{
    private readonly IStringLocalizer<AcceptCampaignNotification> _localizer;
    private readonly ILogger<AcceptCampaignNotification> _logger;
    private readonly IMailService _mailService;
    private readonly IApplicationSettings _settings;

    public AcceptCampaignNotificationHandler(
        ILogger<AcceptCampaignNotification> logger,
        IStringLocalizer<AcceptCampaignNotification> localizer,
        IMailService mailService,
        IApplicationSettings settings)
    {
        _logger = logger;
        _localizer = localizer;
        _mailService = mailService;
        _settings = settings;
    }


    public async Task Handle(AcceptCampaignNotification notification, CancellationToken cancellationToken)
    {
        var sendMailResult = await _mailService.SendAsync(
            notification.Email,
            _localizer["Congratulations"],
            "_campaignaccepted",
            new
            {
                Username=notification.Email,
                notification.Password,
                notification.LoginUrl,
                notification.Name,
                _settings.Company,
            });

        _logger.LogInformation("Congarts email sent to {Email}. sending result {Successful} {Message}",
            notification.Email, sendMailResult.Successful, string.Join(' ', sendMailResult.ErrorMessages));
    }
}


public record DenyCampaignNotification(string Email, string Name)
    : INotification;

public class DenyCampaignNotificationHandler : INotificationHandler<DenyCampaignNotification>
{
    private readonly IStringLocalizer<DenyCampaignNotificationHandler> _localizer;
    private readonly ILogger<DenyCampaignNotificationHandler> _logger;
    private readonly IMailService _mailService;
    private readonly IApplicationSettings _settings;

    public DenyCampaignNotificationHandler(
        ILogger<DenyCampaignNotificationHandler> logger,
        IStringLocalizer<DenyCampaignNotificationHandler> localizer,
        IMailService mailService,
        IApplicationSettings settings)
    {
        _logger = logger;
        _localizer = localizer;
        _mailService = mailService;
        _settings = settings;
    }


    public async Task Handle(DenyCampaignNotification notification, CancellationToken cancellationToken)
    {
        var sendMailResult = await _mailService.SendAsync(
            notification.Email,
            _localizer["Campaign Request Update"],
            "_campaignrejected",
            new
            {
                Username = notification.Email,
                notification.Name,
                _settings.Company,
                GuidelinesUrl=""
            });

        _logger.LogInformation("Deny Campaign email sent to {Email}. sending result {Successful} {Message}",
            notification.Email, sendMailResult.Successful, string.Join(' ', sendMailResult.ErrorMessages));
    }
}