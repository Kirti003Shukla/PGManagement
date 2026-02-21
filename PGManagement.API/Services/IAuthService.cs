using PGManagement.API.DTOs;

namespace PGManagement.API.Services
{
    public interface IAuthService
    {
        LoginResponseDto Login(LoginRequestDto request);
        Task<FirebaseLoginResponseDto> FirebaseLoginAsync(FirebaseLoginRequestDto request);
    }
}