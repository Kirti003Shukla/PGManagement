namespace PGManagement.API.DTOs;

public class TenantLoginResponseDto
{
    public int TenantId { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string Role { get; set; } = "Tenant";
    public bool IsApproved { get; set; }
    public bool ProfileComplete { get; set; }
    public string Message { get; set; } = string.Empty;
}
