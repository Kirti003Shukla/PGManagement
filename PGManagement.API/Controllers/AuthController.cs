using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PGManagement.API.Data;
using PGManagement.API.DTOs;
using PGManagement.API.Models;
using PGManagement.API.Services;
using System.Security.Claims;
using Microsoft.Extensions.Hosting;

namespace PGManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {

        private readonly IAuthService _authService;
        private readonly PGManagementDbContext _dbContext;
        private readonly ILogger<AuthController> _logger;
        private readonly IHostEnvironment _environment;

        public AuthController(IAuthService authService, PGManagementDbContext dbContext, ILogger<AuthController> logger, IHostEnvironment environment)
        {
            _authService = authService;
            _dbContext = dbContext;
            _logger = logger;
            _environment = environment;
        }
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequestDto request)
        {
            var response = _authService.Login(request);
            if (!response.IsSuccess)
            {
                return Unauthorized(response);
            }

            return Ok(response);
        }

        [HttpPost("admin-login")]
        public IActionResult AdminLogin([FromBody] LoginRequestDto request)
        {
            return Login(request);
        }

        [HttpPost("firebase-login")]
        public async Task<IActionResult> FirebaseLogin([FromBody] FirebaseLoginRequestDto request)
        {
            try
            {
                var response = await _authService.FirebaseLoginAsync(request);
                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid firebase-login request payload.");
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Firebase authentication failed.");
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error during firebase-login.");
                return Problem(title: "Internal server error", detail: _environment.IsDevelopment() ? ex.Message : null);
            }
        }

        [HttpPost("tenant-login")]
        public async Task<IActionResult> TenantLogin()
        {
            var uid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var phoneNumber = User.FindFirst("firebase:phone_number")?.Value;

            if (string.IsNullOrWhiteSpace(uid) || string.IsNullOrWhiteSpace(phoneNumber))
            {
                return BadRequest(new { message = "Phone number was not found in the Firebase token." });
            }

            Tenant? tenant;
            try
            {
                tenant = await _dbContext.Tenants
                    .FirstOrDefaultAsync(t => t.FirebaseUid == uid);

                if (tenant == null)
                {
                    tenant = new Tenant
                    {
                        FirebaseUid = uid,
                        PhoneNumber = phoneNumber,
                        IsApproved = false,
                        ApprovedAtUtc = null,
                        CreatedAtUtc = DateTime.UtcNow,
                        UpdatedAtUtc = DateTime.UtcNow
                    };

                    _dbContext.Tenants.Add(tenant);
                }
                else
                {
                    if (!string.Equals(tenant.PhoneNumber, phoneNumber, StringComparison.Ordinal))
                    {
                        tenant.PhoneNumber = phoneNumber;
                    }

                    tenant.UpdatedAtUtc = DateTime.UtcNow;
                }

                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database update failed during tenant-login for uid {Uid}.", uid);
                return Problem(title: "Database error", detail: _environment.IsDevelopment() ? ex.Message : null);
            }
            catch (SqlException ex) when (ex.Number == -2 || ex.Message.Contains("timeout", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogError(ex, "SQL timeout during tenant-login for uid {Uid}.", uid);
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new
                {
                    message = "Database is taking too long to respond. Please retry in a few seconds."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error during tenant-login for uid {Uid}.", uid);
                return Problem(title: "Internal server error", detail: _environment.IsDevelopment() ? ex.Message : null);
            }

            var profileComplete = !string.IsNullOrWhiteSpace(tenant.FullName)
                && !string.IsNullOrWhiteSpace(tenant.Email)
                && tenant.JoinDate.HasValue;

            return Ok(new TenantLoginResponseDto
            {
                TenantId = tenant.Id,
                PhoneNumber = tenant.PhoneNumber,
                IsApproved = tenant.IsApproved,
                ProfileComplete = profileComplete,
                Message = tenant.IsApproved
                    ? "Login successful"
                    : "Your account is pending admin approval."
            });
        }

        [HttpGet("logout")]
        public IActionResult Logout()
        {
            return Ok("Logout endpoint");
        }
    }
}