using AutoMapper;
using Microsoft.AspNet.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using AddOptimization.Data.Contracts;
using AddOptimization.Data.Entities;
using AddOptimization.Contracts.Services;
using AddOptimization.Contracts.Dto;
using AddOptimization.Utilities.Common;
using AddOptimization.Utilities.Extensions;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using AddOptimization.Utilities.Constants;
using AddOptimization.Utilities.Interface;
using AddOptimization.Contracts.Constants;
using AddOptimization.Utilities.Models;
using System.Security.Cryptography;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;

namespace AddOptimization.Services.Services;

public class AuthService : IAuthService
{
    private readonly IConfiguration _configuration;
    private readonly ITemplateService _templateService;
    private readonly ILogger<AuthService> _logger;
    private readonly IMapper _mapper;
    private readonly IGenericRepository<ApplicationUser> _applicationUserRepository;
    private readonly IGenericRepository<Employee> _employeeRepository;
    private readonly IGenericRepository<RefreshToken> _refreshTokenRepository;
    private readonly IGenericRepository<PasswordResetToken> _passwordResetTokenRepository;
    private readonly IEmailService _emailService;
    private readonly IPermissionService _permissionService;
    public AuthService(IConfiguration configuration, ILogger<AuthService> logger,
        IGenericRepository<ApplicationUser> applicationUserRepository,
        IMapper mapper, IGenericRepository<RefreshToken> tokenRepository,
        IGenericRepository<PasswordResetToken> passwordResetTokenRepository,
        IEmailService emailService,
        ITemplateService templateService,
        IPermissionService permissionService,
        IGenericRepository<Employee> employeeRepository)
    {
        _logger = logger;
        _applicationUserRepository = applicationUserRepository;
        _mapper = mapper;
        _refreshTokenRepository = tokenRepository;
        _passwordResetTokenRepository = passwordResetTokenRepository;
        _emailService = emailService;
        _templateService = templateService;
        _configuration = configuration;
        _permissionService = permissionService;
        _employeeRepository = employeeRepository;
    }

    private (string token, DateTime expiry) GenerateAccessToken(ApplicationUser entity)
    {
        try
        {
            var issuer = _configuration["Jwt:Issuer"];
            var audience = _configuration["Jwt:Audience"];
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
            var expiry = DateTime.UtcNow.AddDays(1);
            var name = entity.FullName;
            var claims = new[]
                {
                    new Claim("Id", entity.Id.ToString()),
                    new Claim(JwtRegisteredClaimNames.Sub, entity.Email),
                    new Claim(ClaimTypes.Name, name),
                    new Claim(JwtRegisteredClaimNames.Email, entity.Email),
                    new Claim(JwtRegisteredClaimNames.Jti,
                    Guid.NewGuid().ToString())
            };
            var roleClaims = entity.UserRoles.Select(ur => new Claim(ClaimTypes.Role, ur.Role.Name)).ToArray();
            claims = claims.Concat(roleClaims).ToArray();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expiry,
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials
                (new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha512Signature)
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var jwtToken = tokenHandler.WriteToken(token);
            return (jwtToken, expiry);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex);
            throw;
        }
    }
    public async Task<ApiResult<AuthResponseDto>> RefreshToken(Guid refreshToken)
    {
        try
        {
            var tokenEntity = await _refreshTokenRepository.FirstOrDefaultAsync(u => !u.IsExpired && u.Token == refreshToken, include: entities => entities.Include(e => e.ApplicationUser).Include(e => e.ApplicationUser).ThenInclude(u => u.UserRoles).ThenInclude(r => r.Role), disableTracking: false);
            if (tokenEntity == null)
            {
                return ApiResult<AuthResponseDto>.Failure(ValidationCodes.InvalidToken);
            }
            var entity = tokenEntity.ApplicationUser;
            var resp = await BuildResponse(entity);
            tokenEntity.IsExpired = false;
            tokenEntity.ExpiredAt = DateTime.UtcNow;
            await _refreshTokenRepository.UpdateAsync(tokenEntity);
            return ApiResult<AuthResponseDto>.Success(resp);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex);
            throw;
        }
    }
    public async Task<ApiResult<bool>> Logout(int? applicationUserId = null)
    {
        try
        {
            var userId = applicationUserId ?? _applicationUserRepository.CurrentUserId;
            var tokens = (await _refreshTokenRepository.QueryAsync(r => r.ApplicationUserId == userId && !r.IsExpired)).ToList();
            if (tokens.Any())
            {
                tokens.ForEach(t =>
                {
                    t.IsExpired = true;
                    t.ExpiredAt = DateTime.UtcNow;
                });
                await _refreshTokenRepository.BulkUpdateAsync(tokens);
            }
            return ApiResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex);
            throw;
        }
    }

    private async Task<AuthResponseDto> BuildResponse(ApplicationUser entity)
    {
        var (token, expiry) = GenerateAccessToken(entity);
        var mappedEntity = _mapper.Map<UserSummaryDto>(entity);
        var newTokenEntity = new RefreshToken
        {
            Token = Guid.NewGuid(),
            ApplicationUserId = entity.Id,
            IsExpired = false
        };
        await _refreshTokenRepository.InsertAsync(newTokenEntity);
        var resp = new AuthResponseDto
        {
            JWT = token,
            RefreshToken = newTokenEntity.Token,
            User = mappedEntity,
            Expiry = expiry
        };
        return resp;
    }

    public async Task<ApiResult<AuthResponseDto>> Login(LoginDto model)
    {
        try
        {
            var username = model.UserName;
            var password = model.Password;
            var entity = await _applicationUserRepository.FirstOrDefaultAsync(u => u.Password != null && (u.UserName.ToLower() == username.ToLower() || u.Email.ToLower() == username.ToLower()), disableTracking: false, include: entities => entities.Include(u => u.UserRoles).ThenInclude(r => r.Role));
            if (entity == null)
            {
                return ApiResult<AuthResponseDto>.Failure(ValidationCodes.InvalidUserName);
            }
            if (await IsEmployeeRole(entity))
            {
                return ApiResult<AuthResponseDto>.Failure(ValidationCodes.LoginWithMicrosoftProvider);
            }

            if (!entity.IsActive)
            {
                return ApiResult<AuthResponseDto>.Failure(ValidationCodes.InactiveUserAccount);
            }
            if (entity.IsLocked ?? false)
            {
                return ApiResult<AuthResponseDto>.Failure(ValidationCodes.LockedUserAccount);
            }
            var passwordHasher = new PasswordHasher();
            var passworResults = passwordHasher.VerifyHashedPassword(entity.Password, password);
            if (passworResults == PasswordVerificationResult.Failed)
            {
                entity.FailedLoginAttampts = (entity.FailedLoginAttampts ?? 0) + 1;
                if (entity.FailedLoginAttampts >= 4)
                {
                    entity.IsLocked = true;
                    await _applicationUserRepository.UpdateAsync(entity);
                    return ApiResult<AuthResponseDto>.Failure(ValidationCodes.UserAccountLockedDueToFailedAttamps);
                }
                await _applicationUserRepository.UpdateAsync(entity);
                var attmaptLeft = 4 - entity.FailedLoginAttampts;
                var message = ValidationMessages.GetValidationMessage(ValidationCodes.InvalidPassword);
                message = message.Replace("[AttemptLeft]", attmaptLeft.ToString());
                return ApiResult<AuthResponseDto>.Failure(ValidationCodes.InvalidPassword, message);
            }
            var resp = await BuildResponse(entity);
            bool? isNDASigned = await GetNDASignedRequired(entity);
            resp.NDASignedRequired = isNDASigned;
            entity.LastLogin = DateTime.UtcNow;
            entity.FailedLoginAttampts = 0;
            await _applicationUserRepository.UpdateAsync(entity);
            return ApiResult<AuthResponseDto>.Success(resp);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex);
            throw;
        }
    }

    public async Task<ApiResult<AuthResponseDto>> MicrosoftLogin(MicrosoftLoginDto model)
    {
        var issuer = _configuration["Issuer"]; // Read from config
        var audience = _configuration["MicrosoftClientId"];
        var isTokenValid = ValidateIdToken(model?.IdToken, issuer, audience, out var preferredUsername);
        if (!isTokenValid)
            return null;

        var email = preferredUsername;
        var entity = await _applicationUserRepository.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower(), disableTracking: false, include: entities => entities.Include(u => u.UserRoles).ThenInclude(r => r.Role));
        if (entity == null)
        {
            return ApiResult<AuthResponseDto>.Failure(ValidationCodes.InvalidUserName);
        }
        if (!entity.IsActive)
        {
            return ApiResult<AuthResponseDto>.Failure(ValidationCodes.InactiveUserAccount);
        }
        if (entity.IsLocked ?? false)
        {
            return ApiResult<AuthResponseDto>.Failure(ValidationCodes.LockedUserAccount);
        }

        var resp = await BuildResponse(entity);
        bool? isNDASigned = await GetNDASignedRequired(entity);
        resp.NDASignedRequired = isNDASigned;
        entity.LastLogin = DateTime.UtcNow;
        entity.FailedLoginAttampts = 0;
        await _applicationUserRepository.UpdateAsync(entity);
        return ApiResult<AuthResponseDto>.Success(resp);
    }

    private async Task<bool?> GetNDASignedRequired(ApplicationUser entity)
    {
        var isEmployeeRole = entity.UserRoles.Any(c => c.Role.Name.Contains("Employee", StringComparison.InvariantCultureIgnoreCase));
        if (isEmployeeRole)
        {
            var employee = await _employeeRepository.FirstOrDefaultAsync(u => u.UserId == entity.Id);
            return employee != null ? !employee.IsNDASigned : false;
        }
        return null;
    }
    private async Task<bool> IsEmployeeRole(ApplicationUser entity)
    {
        return entity.UserRoles.Any(c => c.Role.Name.Contains("Employee", StringComparison.InvariantCultureIgnoreCase));
    }
    public bool ValidateIdToken(string token, string expectedIssuer, string expectedAudience, out string preferredUsername)
    {
        try
        {
            // Decode the token
            var handler = new JwtSecurityTokenHandler();
            var parsedToken = handler.ReadJwtToken(token);

            // Validate required fields
            if (!parsedToken.Issuer.Contains(expectedIssuer))
            {
                preferredUsername = "";
                return false;
            }

            if (parsedToken.Audiences.FirstOrDefault() != expectedAudience)
            {
                preferredUsername = "";
                return false;
            }

            var claims = (List<Claim>)parsedToken.Claims;
            var preferredUsernameValue = claims.FirstOrDefault(c => c.Type == "preferred_username")?.Value;
            if (preferredUsernameValue == null || preferredUsernameValue == "")
            {
                preferredUsername = "";
                return false;
            }
            var expiry = claims.FirstOrDefault(c => c.Type == "exp")?.Value;
            DateTimeOffset expiryTime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(expiry));
            if (expiryTime < DateTime.UtcNow)
            {
                preferredUsername = "";
                return false; // Token has expired
            }

            // Add additional validation for custom fields if needed
            preferredUsername = preferredUsernameValue;
            return true; // All validations passed
        }
        catch (SecurityTokenException ex)
        {
            // Handle token validation errors
            Console.WriteLine($"Token validation failed: {ex.Message}");
            preferredUsername = "";
            return false;
        }
    }


    private static string GenerateToken()
    {
        byte[] guidBytes = Guid.NewGuid().ToByteArray();
        return Convert.ToBase64String(guidBytes);
    }
    public async Task<ApiResult<bool>> SendForgotPasswordLink(ForgotPasswordDto model)
    {
        try
        {
            var email = model.Email;
            var userEntity = await _applicationUserRepository.FirstOrDefaultAsync(a => a.Email.ToLower() == email.ToLower());
            if (userEntity == null)
            {
                return ApiResult<bool>.Failure(ValidationCodes.EmailNotExists);
            }
            if (!userEntity.IsActive)
            {
                return ApiResult<bool>.Failure(ValidationCodes.InactiveUserAccount);
            }
            var isLimitReached = (await _passwordResetTokenRepository.QueryAsync(e => e.CreatedByUserId == userEntity.Id && !e.IsExpired
            && e.CreatedAt > DateTime.UtcNow.Date)).Count() >= 3;
            if (isLimitReached)
            {
                return ApiResult<bool>.Failure(ValidationCodes.PasswordResetLinkLimitReached);
            }
            string token = GenerateToken();
            var entity = new PasswordResetToken
            {
                Token = token,
                ExpiryDate = DateTime.UtcNow.AddDays(1),
                CreatedByUserId = userEntity.Id
            };
            entity = await _passwordResetTokenRepository.InsertAsync(entity);
            var isEmailSent = await SendPasswordResetEmail(token, userEntity.FullName, email);
            if (!isEmailSent)
            {
                await _passwordResetTokenRepository.DeleteAsync(entity);
                return ApiResult<bool>.Failure(ValidationCodes.IssueSendingEmail);

            }
            return ApiResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex);
            throw;
        }
    }
    public async Task<ApiResult<bool>> ValidateResetPasswordToken(ResetPasswordDto model)
    {
        try
        {
            var retVal = (await GetTokenUserId(model.Token)) != null;
            return ApiResult<bool>.Success(retVal);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex);
            throw;
        }
    }
    private async Task<int?> GetTokenUserId(string token)
    {
        return (await _passwordResetTokenRepository.FirstOrDefaultAsync(x => x.Token == token && !x.IsExpired && x.ExpiryDate >= DateTime.UtcNow))?.CreatedByUserId;
    }
    public async Task<ApiResult<bool>> ResetPassword(ResetPasswordDto model, bool? isAdminMode = null)
    {
        try
        {

            var tokenUserId = (isAdminMode ?? false) ? model.UserId : await GetTokenUserId(model.Token);
            if (tokenUserId == null)
            {
                return ApiResult<bool>.Failure(ValidationCodes.PasswordResetLinkExpired);
            }
            var userEntity = await _applicationUserRepository.FirstOrDefaultAsync(e => e.Id == tokenUserId);
            if (userEntity == null)
            {
                return ApiResult<bool>.NotFound("User");
            }
            var passwordHasher = new PasswordHasher();
            string hashedPassword = passwordHasher.HashPassword(model.Password);
            userEntity.IsLocked = false;
            userEntity.FailedLoginAttampts = 0;
            userEntity.Password = hashedPassword;
            await _applicationUserRepository.UpdateAsync(userEntity);
            var userTokens = (await _passwordResetTokenRepository.QueryAsync(e => e.CreatedByUserId == tokenUserId && !e.IsExpired)).ToList();
            foreach (var item in userTokens)
            {
                item.IsExpired = true;
                item.ExpiryDate = DateTime.UtcNow;
            }
            await _passwordResetTokenRepository.BulkUpdateAsync(userTokens);

            return ApiResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex);
            throw;
        }
    }
    private async Task<bool> SendPasswordResetEmail(string token, string userFullName, string email)
    {
        try
        {
            var subject = "Reset Your Account Password";
            var emailTemplate = _templateService.ReadTemplate(EmailTemplates.ResetPassword);
            var resetLink = (_configuration.ReadSection<AppUrls>(AppSettingsSections.AppUrls).ResetPassword).Replace("[token]", token);
            emailTemplate = emailTemplate.Replace("[UserFullName]", userFullName).Replace("[PasswordResetLink]", resetLink);
            return await _emailService.SendEmail(email, subject, emailTemplate);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex);
            return false;
        }
    }
}
