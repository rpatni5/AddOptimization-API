using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using AddOptimization.Utilities.Interface;
using System;
using System.IO;

namespace AddOptimization.Utilities.Services;

public class TemplateService : ITemplateService
{
    private readonly IHostEnvironment _hostingEnvironment;
    private readonly ILogger<TemplateService> _logger;

    public TemplateService(IHostEnvironment hostingEnvironment, ILogger<TemplateService> logger)
    {
        _hostingEnvironment = hostingEnvironment;
        _logger = logger;
    }

    public string ReadTemplate(string templateName)
    {
        try
        {
            templateName = Path.GetFileName(templateName);
            var emailTemplatePath = Path.Combine(_hostingEnvironment.ContentRootPath, "Assets", "Templates");
            if (!Directory.Exists(emailTemplatePath))
            {
                Directory.CreateDirectory(emailTemplatePath);
            }
            var filePath = Path.Combine(emailTemplatePath, templateName);
            if (File.Exists(filePath))
            {
                return File.ReadAllText(filePath);
            }
            else
            {
                _logger.LogWarning($"Template file not found: {filePath}");
                return string.Empty;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error reading template: {ex.Message}");
            throw; 
        }
    }
}
