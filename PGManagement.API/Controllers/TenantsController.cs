using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PGManagement.API.Data;
using PGManagement.API.DTOs;
using System.Security.Claims;

namespace PGManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TenantsController : ControllerBase
{
    private readonly PGManagementDbContext _dbContext;

    public TenantsController(PGManagementDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetProfile()
    {
        var uid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrWhiteSpace(uid))
        {
            return Unauthorized(new { message = "Firebase user not found." });
        }

        var tenant = await _dbContext.Tenants.FirstOrDefaultAsync(t => t.FirebaseUid == uid);

        if (tenant == null)
        {
            return NotFound(new { message = "Tenant profile not found." });
        }

        if (!tenant.IsApproved)
        {
            return StatusCode(403, new { message = "Your account is pending admin approval." });
        }

        return Ok(new TenantProfileResponseDto
        {
            TenantId = tenant.Id,
            PhoneNumber = tenant.PhoneNumber,
            FullName = tenant.FullName,
            Email = tenant.Email,
            JoinDate = tenant.JoinDate,
            AdvanceAmount = tenant.AdvanceAmount,
            RoomRent = tenant.RoomRent,
            IdProofType = tenant.IdProofType,
            IdProofNumber = tenant.IdProofNumber
        });
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateProfile([FromBody] TenantProfileDto request)
    {
        var uid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrWhiteSpace(uid))
        {
            return Unauthorized(new { message = "Firebase user not found." });
        }

        var tenant = await _dbContext.Tenants.FirstOrDefaultAsync(t => t.FirebaseUid == uid);

        if (tenant == null)
        {
            return NotFound(new { message = "Tenant profile not found." });
        }

        if (!tenant.IsApproved)
        {
            return StatusCode(403, new { message = "Your account is pending admin approval." });
        }

        tenant.FullName = request.FullName;
        tenant.Email = request.Email;
        tenant.JoinDate = request.JoinDate;
        tenant.AdvanceAmount = request.AdvanceAmount;
        tenant.RoomRent = request.RoomRent;
        tenant.IdProofType = request.IdProofType;
        tenant.IdProofNumber = request.IdProofNumber;
        tenant.UpdatedAtUtc = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return Ok(new TenantProfileResponseDto
        {
            TenantId = tenant.Id,
            PhoneNumber = tenant.PhoneNumber,
            FullName = tenant.FullName,
            Email = tenant.Email,
            JoinDate = tenant.JoinDate,
            AdvanceAmount = tenant.AdvanceAmount,
            RoomRent = tenant.RoomRent,
            IdProofType = tenant.IdProofType,
            IdProofNumber = tenant.IdProofNumber
        });
    }
}
