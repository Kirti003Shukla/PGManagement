namespace PGManagement.API.DTOs
{
    public class LoginResponseDto
    {
        public string Role { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public bool IsSuccess { get; set; }
    }
}