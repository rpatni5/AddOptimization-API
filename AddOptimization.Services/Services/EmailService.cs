using System.Net.Mail;
using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using AddOptimization.Utilities.Models;
using AddOptimization.Utilities.Extensions;
using AddOptimization.Utilities.Helpers;
using AddOptimization.Contracts.Services;
using AddOptimization.Utilities.Constants;
using AddOptimization.Contracts.Constants;
using AddOptimization.Contracts.Dto;
using AddOptimization.Utilities.Common;

namespace AddOptimization.Services.Services;

public  class EmailService: IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;
    private readonly ISettingService _settingService;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger, ISettingService settingService)
    {
        _configuration = configuration;
        _logger = logger;
        _settingService = settingService;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="recipientEmails">Semicolon separated list of email address</param>
    /// <param name="cc">Semicolon separated list of email address</param>
    /// <param name="subject"></param>
    /// <param name="body"></param>
    /// <param name="hasHtml"></param>
    /// <returns></returns>
    public async Task<bool> SendEmail(string recipientEmails,string subject,string body,string cc=null,bool hasHtml= true)
    {
        try
        {
            var isEmailNotificationsEnabled = await _settingService.GetSettingByCode(SettingCodes.EMAIL_NOTIFICATIONS);
            if (isEmailNotificationsEnabled != null && !isEmailNotificationsEnabled.Result.IsEnabled)
            {
                _logger.LogInformation($"Email notification setting is disabled. Details : Email recipient : {recipientEmails}, Subject: {subject},   Body : {body}.");
                return false;
            }
            SmtpClient smtpClient;
            MailMessage mailMessage;
            Task.Run(() => Send(recipientEmails, subject, body, cc, hasHtml, out smtpClient, out mailMessage));
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogException(ex);
            return false;
        }
    }

    private void Send(string recipientEmails, string subject, string body, string cc, bool hasHtml, out SmtpClient smtpClient, out MailMessage mailMessage, string fromEmail = null)
    {
        var emailSettings = _configuration.ReadSection<EmailSettings>(AppSettingsSections.EmailSettings);
        string smtpServer = emailSettings.SMTPServer;
        int smtpPort = emailSettings.SMTPPort;
        string senderEmail =  emailSettings.SenderEmail;
        string senderPassword = emailSettings.SenderPassword;
        smtpClient = new SmtpClient(smtpServer, smtpPort)
        {
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(senderEmail, senderPassword),
            EnableSsl = true,
            Timeout = 60000
        };
        mailMessage = new MailMessage()
        {
            Subject = subject,
            Body = body,
            From = new MailAddress(fromEmail ?? senderEmail),
            IsBodyHtml = hasHtml
        };
        if(!string.IsNullOrEmpty(recipientEmails))
        {
            foreach (var item in recipientEmails.Split(';'))
            {
                if (EmailHelper.IsValidEmail(item))
                {
                    mailMessage.To.Add(new MailAddress(item));
                }
            }
            if (!string.IsNullOrEmpty(cc))
            {
                foreach (var item in cc.Split(';'))
                {
                    if (EmailHelper.IsValidEmail(item))
                    {
                        mailMessage.CC.Add(new MailAddress(item));
                    }
                }
            }
            smtpClient.Send(mailMessage);

        }
    }



    public async Task<bool> SendEmailSync(string recipientEmails, string subject, string body, string cc = null, bool hasHtml = true, string fromEmail = null)
    {
        try
        {
            var isEmailNotificationsEnabled = await _settingService.GetSettingByCode(SettingCodes.EMAIL_NOTIFICATIONS);
            if (isEmailNotificationsEnabled != null && !isEmailNotificationsEnabled.Result.IsEnabled)
            {
                _logger.LogInformation($"Email notification setting is disabled. Details : Email recipient : {recipientEmails}, Subject: {subject},   Body : {body}.");
                return false;
            }
            SmtpClient smtpClient;
            MailMessage mailMessage;
            Send(recipientEmails, subject, body, cc, hasHtml, out smtpClient, out mailMessage, fromEmail);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogException(ex);
            return false;
        }
    }
}
