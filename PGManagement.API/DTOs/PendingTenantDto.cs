namespace PGManagement.API.DTOs;

public sealed class PendingTenantDto
{
    public int TenantId { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
}
