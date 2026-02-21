namespace PGManagement.API.DTOs;

public sealed class MachineStatusDto
{
    public int MachineId { get; set; }
    public string MachineName { get; set; } = string.Empty;
    public bool HasActiveSession { get; set; }
    public int? RemainingMinutes { get; set; }
    public bool IsAvailable { get; set; }
    public int? CurrentUserId { get; set; }
}
