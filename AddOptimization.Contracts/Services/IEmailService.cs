﻿namespace AddOptimization.Contracts.Services;

public interface IEmailService
{
    Task<bool> SendEmail(string recipientEmails, string subject, string body, string cc = null, bool hasHtml = true);
}