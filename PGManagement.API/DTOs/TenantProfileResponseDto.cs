namespace PGManagement.API.DTOs;

public class TenantProfileResponseDto
{
    public int TenantId { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public DateTime? JoinDate { get; set; }
    public decimal? AdvanceAmount { get; set; }
    public decimal? RoomRent { get; set; }
    public string? IdProofType { get; set; }
    public string? IdProofNumber { get; set; }
}
