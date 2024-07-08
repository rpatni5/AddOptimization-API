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
            var emailSettings = _configuration.ReadSection<EmailSettings>(AppSettingsSections.EmailSettings);
            string smtpServer = emailSettings.SMTPServer;
            int smtpPort = emailSettings.SMTPPort;
            string senderEmail = emailSettings.SenderEmail;
            string senderPassword = emailSettings.SenderPassword;
            using var smtpClient = new SmtpClient(smtpServer, smtpPort)
            {
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(senderEmail, senderPassword),
                EnableSsl = true,
                Timeout=60000
            };
            using var mailMessage = new MailMessage()
            {
                Subject = subject,
                Body = body,
                From = new MailAddress(senderEmail),
                IsBodyHtml = hasHtml
            };
            foreach (var item in recipientEmails.Split(';'))
            {
                if(EmailHelper.IsValidEmail(item))
                {
                    mailMessage.To.Add(new MailAddress(item));  
                }
            };
            if (!string.IsNullOrEmpty(cc))
            {
                foreach (var item in cc.Split(';'))
                {
                    if (EmailHelper.IsValidEmail(item))
                    {
                        mailMessage.CC.Add(new MailAddress(item));
                    }
                };
            }
            smtpClient.Send(mailMessage);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogException(ex);
            return false;
        }
    }
}
