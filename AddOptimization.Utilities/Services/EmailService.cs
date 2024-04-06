using System.Net.Mail;
using System.Net;
using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using AddOptimization.Utilities.Models;
using AddOptimization.Utilities.Extensions;
using AddOptimization.Utilities.Helpers;
using AddOptimization.Utilities.Interface;
using AddOptimization.Utilities.Constants;
using System.Threading.Tasks;

namespace AddOptimization.Utilities.Services;

public  class EmailService: IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;
    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="recipientEmails">Semicolon seprated list of email address</param>
    /// <param name="cc">Semicolon seprated list of email address</param>
    /// <param name="subject"></param>
    /// <param name="body"></param>
    /// <param name="hasHtml"></param>
    /// <returns></returns>
    public async Task<bool> SendEmail(string recipientEmails,string subject,string body,string cc=null,bool hasHtml= true)
    {
        try
        {
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
            return await Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex);
            return false;
        }
    }
}
