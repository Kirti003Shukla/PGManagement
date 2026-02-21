namespace PGManagement.API.DTOs;

public sealed class FirebaseLoginResponseDto
{
    public string Token { get; set; } = string.Empty;
    public int TenantId { get; set; }
    public string Role { get; set; } = string.Empty;
}
