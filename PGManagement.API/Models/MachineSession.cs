namespace PGManagement.API.Models;

public class MachineSession
{
    public int SessionId { get; set; }

    public int MachineId { get; set; }
    public Machine Machine { get; set; } = null!;

    public int UserId { get; set; }
    public Tenant User { get; set; } = null!;

    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }

    public MachineSessionStatus Status { get; set; } = MachineSessionStatus.Active;
}
