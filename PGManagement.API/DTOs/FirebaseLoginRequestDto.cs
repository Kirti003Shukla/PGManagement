using System.ComponentModel.DataAnnotations;

namespace PGManagement.API.DTOs;

public sealed class FirebaseLoginRequestDto
{
    [Required]
    public string IdToken { get; set; } = string.Empty;
}
