using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PGManagement.API.DTOs;
using PGManagement.API.Services;
using System.Security.Claims;

namespace PGManagement.API.Controllers;

[ApiController]
[Route("api/machines")]
[Authorize]
public sealed class MachinesController : ControllerBase
{
    private readonly IMachineStatusService _machineService;
    private readonly ILogger<MachinesController> _logger;

    public MachinesController(IMachineStatusService machineService, ILogger<MachinesController> logger)
    {
        _machineService = machineService;
        _logger = logger;
    }

    [HttpGet("status")]
    [ProducesResponseType(typeof(List<MachineStatusDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<MachineStatusDto>>> GetStatuses()
    {
        var statuses = await _machineService.GetMachineStatusesAsync();
        return Ok(statuses);
    }

    [HttpPost("{machineId:int}/start")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> StartSession([FromRoute] int machineId, [FromBody] StartMachineSessionRequestDto request)
    {
        var tenantIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(tenantIdClaim, out var tenantId))
        {
            return Unauthorized(new { message = "Invalid or missing tenant identity." });
        }

        try
        {
            await _machineService.StartSessionAsync(machineId, tenantId, request.DurationMinutes);
            return Ok();
        }
        catch (MachineBusyException ex)
        {
            _logger.LogWarning(ex, "Machine {MachineId} is already in use.", machineId);
            return Conflict(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid start session request for machine {MachineId}.", machineId);
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{machineId:int}/complete")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CompleteSession([FromRoute] int machineId)
    {
        var tenantIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(tenantIdClaim, out var tenantId))
        {
            return Unauthorized(new { message = "Invalid or missing tenant identity." });
        }

        var isAdmin = User.FindAll(ClaimTypes.Role)
            .Any(claim => string.Equals(claim.Value, "Admin", StringComparison.OrdinalIgnoreCase));

        try
        {
            await _machineService.CompleteSessionAsync(machineId, tenantId, isAdmin);
            return Ok();
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Tenant {TenantId} is not allowed to complete session for machine {MachineId}.", tenantId, machineId);
            return Forbid();
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Machine {MachineId} not found while completing session.", machineId);
            return NotFound(new { message = ex.Message });
        }
    }
}
