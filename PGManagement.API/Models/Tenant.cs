namespace PGManagement.API.Models;

public class Tenant
{
    public int Id { get; set; }
    public string FirebaseUid { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public bool IsApproved { get; set; }
    public DateTime? ApprovedAtUtc { get; set; }
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public DateTime? JoinDate { get; set; }
    public decimal? AdvanceAmount { get; set; }
    public decimal? RoomRent { get; set; }
    public string? IdProofType { get; set; }
    public string? IdProofNumber { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}
