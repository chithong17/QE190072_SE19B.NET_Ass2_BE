using System.Threading.Tasks;
using TaskTrack.Repo.Models;
using TaskTrack.Service.DTOs;

namespace TaskTrack.Service.Interfaces
{
    public interface IAuthService
    {
        Task<(bool Success, string? ErrorMessage, AccountResponseDto? Account)> RegisterStaffAsync(RegisterRequestDto dto);
        Task<(bool Success, string? ErrorMessage, AuthResponseDto? Auth)> LoginAsync(LoginRequestDto dto);
        Task<(bool Success, string? ErrorMessage, AuthResponseDto? Auth)> RefreshTokenAsync(RefreshTokenRequestDto dto);
        Task<(bool Success, string? ErrorMessage)> RevokeRefreshTokenAsync(int accountId);
        string GenerateJwtToken(SystemAccount account);
        Task<(bool Success, string? ErrorMessage)> ChangePasswordAsync(int accountId, ChangePasswordRequestDto dto);
        Task<(bool Success, string? ErrorMessage, AccountResponseDto? Account)> UpdateProfileAsync(int accountId, UpdateProfileRequestDto dto);
    }
}
