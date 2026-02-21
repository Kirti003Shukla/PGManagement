using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PGManagement.API.Data;
using PGManagement.API.DTOs;
using System.Text;

namespace PGManagement.API.Controllers;

[ApiController]
[Route("api/admin/tenants")]
public sealed class AdminTenantsController : ControllerBase
{
    private readonly PGManagementDbContext _dbContext;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AdminTenantsController> _logger;

    public AdminTenantsController(PGManagementDbContext dbContext, IConfiguration configuration, ILogger<AdminTenantsController> logger)
    {
        _dbContext = dbContext;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpGet("pending")]
    public async Task<IActionResult> GetPendingTenants()
    {
        if (!TryValidateAdminRequest(Request, _configuration, out var errorMessage))
        {
            return Unauthorized(new { message = errorMessage });
        }

        var pending = await _dbContext.Tenants
            .Where(t => !t.IsApproved)
            .OrderByDescending(t => t.CreatedAtUtc)
            .Select(t => new PendingTenantDto
            {
                TenantId = t.Id,
                PhoneNumber = t.PhoneNumber,
                CreatedAtUtc = t.CreatedAtUtc
            })
            .ToListAsync();

        return Ok(pending);
    }

    [HttpPost("{tenantId:int}/approve")]
    public async Task<IActionResult> ApproveTenant([FromRoute] int tenantId)
    {
        if (!TryValidateAdminRequest(Request, _configuration, out var errorMessage))
        {
            return Unauthorized(new { message = errorMessage });
        }

        var tenant = await _dbContext.Tenants.FirstOrDefaultAsync(t => t.Id == tenantId);
        if (tenant == null)
        {
            return NotFound(new { message = "Tenant not found." });
        }

        if (!tenant.IsApproved)
        {
            tenant.IsApproved = true;
            tenant.ApprovedAtUtc = DateTime.UtcNow;
            tenant.UpdatedAtUtc = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();
        }

        return Ok(new { message = "Tenant approved." });
    }

    private bool TryValidateAdminRequest(HttpRequest request, IConfiguration configuration, out string errorMessage)
    {
        errorMessage = "Invalid admin credentials.";

        var auth = request.Headers.Authorization.ToString();
        if (string.IsNullOrWhiteSpace(auth))
        {
            auth = request.Headers["X-Admin-Authorization"].ToString();
        }

        if (string.IsNullOrWhiteSpace(auth))
        {
            errorMessage = "Missing admin authorization header.";
            _logger.LogWarning("Admin request missing auth header.");
            return false;
        }

        var parts = auth.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2 || !string.Equals(parts[0], "Basic", StringComparison.OrdinalIgnoreCase))
        {
            errorMessage = "Admin authorization must use Basic auth.";
            _logger.LogWarning("Admin request auth scheme not Basic: {AuthHeader}", parts.Length > 0 ? parts[0] : "(empty)");
            return false;
        }

        var encoded = parts[1].Trim();
        string decoded;
        try
        {
            decoded = Encoding.UTF8.GetString(Convert.FromBase64String(encoded));
        }
        catch
        {
            errorMessage = "Invalid admin Basic auth token.";
            _logger.LogWarning("Admin request had invalid Base64 token.");
            return false;
        }

        var separatorIndex = decoded.IndexOf(':');
        if (separatorIndex <= 0)
        {
            errorMessage = "Invalid admin Basic auth token.";
            _logger.LogWarning("Admin request decoded token missing ':' separator.");
            return false;
        }

        var email = decoded[..separatorIndex];
        var password = decoded[(separatorIndex + 1)..];

        var adminEmail = configuration["AdminAuth:Email"];
        var adminPassword = configuration["AdminAuth:Password"];

        if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
        {
            errorMessage = "Admin credentials are not configured on the server.";
            _logger.LogError("AdminAuth credentials not configured in appsettings.");
            return false;
        }

        var ok = string.Equals(email, adminEmail, StringComparison.OrdinalIgnoreCase)
            && string.Equals(password, adminPassword, StringComparison.Ordinal);

        if (!ok)
        {
            _logger.LogWarning("Admin credentials mismatch for email {Email}.", email);
            errorMessage = "Invalid admin credentials.";
        }

        return ok;
    }
}
