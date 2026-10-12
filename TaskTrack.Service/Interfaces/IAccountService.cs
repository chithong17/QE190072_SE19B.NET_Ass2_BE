using System.Collections.Generic;
using System.Threading.Tasks;
using TaskTrack.Service.DTOs;

namespace TaskTrack.Service.Interfaces
{
    public interface IAccountService
    {
        Task<IEnumerable<AccountResponseDto>> GetAllAccountsAsync();
        Task<AccountResponseDto?> GetAccountByIdAsync(int accountId);
        Task<(bool Success, string? ErrorMessage, AccountResponseDto? Account)> UpdateAccountAsync(int accountId, UpdateAccountRequestDto dto);
        Task<(bool Success, string? ErrorMessage)> DeleteAccountAsync(int accountId);
    }
}
