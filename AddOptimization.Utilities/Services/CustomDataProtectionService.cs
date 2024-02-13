using Microsoft.AspNetCore.DataProtection;

namespace AddOptimization.Utilities.Services;

public class CustomDataProtectionService
{
    private readonly IDataProtector protector;
    private readonly string dataProtectionKey = "AddOptimizationEcryption";
    public CustomDataProtectionService(IDataProtectionProvider dataProtectionProvider)
    {
        protector = dataProtectionProvider.CreateProtector(dataProtectionKey);
    }
    public string Decode(string data)
    {
        return protector.Unprotect(data);
    }
    public string Encode(string data)
    {
        return protector.Protect(data);
    }
}
