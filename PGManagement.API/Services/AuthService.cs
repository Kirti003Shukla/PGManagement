using FirebaseAdmin.Auth;
using Microsoft.EntityFrameworkCore;
using PGManagement.API.DTOs;
using PGManagement.API.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
namespace PGManagement.API.Services;
public class AuthService : IAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;
        private readonly PGManagementDbContext _dbContext;

        public AuthService(IConfiguration configuration, ILogger<AuthService> logger, PGManagementDbContext dbContext)
        {
            _configuration = configuration;
            _logger = logger;
            _dbContext = dbContext;
        }

        public LoginResponseDto Login(LoginRequestDto request)
        {
            var adminEmail = _configuration["AdminAuth:Email"];
            var adminPassword = _configuration["AdminAuth:Password"];

            if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
            {
                return new LoginResponseDto
                {
                    Role = string.Empty,
                    Message = "Admin credentials are not configured.",
                    IsSuccess = false
                };
            }

            var isValid = string.Equals(request.Email, adminEmail, StringComparison.OrdinalIgnoreCase)
                          && string.Equals(request.Password, adminPassword, StringComparison.Ordinal);

            return new LoginResponseDto
            {
                Role = isValid ? "Admin" : string.Empty,
                Message = isValid ? "Login successful" : "Invalid admin credentials.",
                IsSuccess = isValid
            };
           
        }

        public async Task<FirebaseLoginResponseDto> FirebaseLoginAsync(FirebaseLoginRequestDto request)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (string.IsNullOrWhiteSpace(request.IdToken))
            {
                throw new ArgumentException("IdToken is required.", nameof(request.IdToken));
            }

            try
            {
                var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(request.IdToken);

                var firebaseUid = decodedToken.Uid;
                string? phoneNumber = null;
                if (decodedToken.Claims.TryGetValue("phone_number", out var phoneNumberClaim))
                {
                    phoneNumber = phoneNumberClaim?.ToString();
                }

                var tenant = await _dbContext.Tenants.FirstOrDefaultAsync(t => t.FirebaseUid == firebaseUid);
                if (tenant == null)
                {
                    throw new UnauthorizedAccessException("Tenant not found for this Firebase user.");
                }

                var jwtKey = GetJwtSetting("Key");
                var issuer = GetJwtSetting("Issuer");
                var audience = GetJwtSetting("Audience");
                var expiryMinutesRaw = GetJwtSetting("ExpiryMinutes");

                if (!int.TryParse(expiryMinutesRaw, out var expiryMinutes) || expiryMinutes <= 0)
                {
                    throw new InvalidOperationException("JWT ExpiryMinutes configuration is invalid.");
                }

                var tenantRole = "Tenant";
                var claims = new List<Claim>
                {
                    new(ClaimTypes.NameIdentifier, tenant.Id.ToString()),
                    new(ClaimTypes.Role, tenantRole)
                };

                var effectivePhone = !string.IsNullOrWhiteSpace(phoneNumber) ? phoneNumber : tenant.PhoneNumber;
                if (!string.IsNullOrWhiteSpace(effectivePhone))
                {
                    claims.Add(new Claim("phone_number", effectivePhone));
                }

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
                var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var expiresAtUtc = DateTime.UtcNow.AddMinutes(expiryMinutes);

                var jwtToken = new JwtSecurityToken(
                    issuer: issuer,
                    audience: audience,
                    claims: claims,
                    expires: expiresAtUtc,
                    signingCredentials: credentials);

                var token = new JwtSecurityTokenHandler().WriteToken(jwtToken);

                _logger.LogInformation("Firebase token verified for uid {FirebaseUid}. Phone claim present: {HasPhoneNumber}",
                    firebaseUid,
                    !string.IsNullOrWhiteSpace(phoneNumber));

                return new FirebaseLoginResponseDto
                {
                    Token = token,
                    TenantId = tenant.Id,
                    Role = tenantRole
                };
            }
            catch (FirebaseAuthException ex)
            {
                _logger.LogWarning(ex, "Firebase token verification failed.");
                throw new UnauthorizedAccessException("Invalid Firebase ID token.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during Firebase login.");
                throw;
            }
        }

        private string GetJwtSetting(string key)
        {
            var value = _configuration[$"Jwt:{key}"]
                ?? _configuration[$"JwtSettings:{key}"]
                ?? _configuration[key];

            if (string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidOperationException($"JWT setting '{key}' is missing.");
            }

            return value;
        }
}