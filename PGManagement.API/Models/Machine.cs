using System.ComponentModel.DataAnnotations;

namespace PGManagement.API.Models;

public class Machine
{
    public int MachineId { get; set; }
    public string MachineName { get; set; } = string.Empty;
    public bool IsAvailable { get; set; } = true;

    public int? CurrentUserId { get; set; }
    public Tenant? CurrentUser { get; set; }

    public DateTime? EndTime { get; set; }

    [Timestamp]
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}
