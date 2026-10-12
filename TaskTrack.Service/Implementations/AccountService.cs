using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaskTrack.Repo.Models;
using TaskTrack.Service.DTOs;
using TaskTrack.Service.Interfaces;

namespace TaskTrack.Service.Implementations
{
    public class AccountService : IAccountService
    {
        private readonly TaskmanagementDbEgrzContext _context;

        public AccountService(TaskmanagementDbEgrzContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<AccountResponseDto>> GetAllAccountsAsync()
        {
            return await _context.SystemAccounts
                .OrderByDescending(a => a.CreatedDate)
                .Select(a => new AccountResponseDto
                {
                    AccountId = a.AccountId,
                    FullName = a.FullName,
                    Email = a.Email,
                    Role = a.Role,
                    CreatedDate = a.CreatedDate
                })
                .ToListAsync();
        }

        public async Task<AccountResponseDto?> GetAccountByIdAsync(int accountId)
        {
            var account = await _context.SystemAccounts.FindAsync(accountId);
            if (account == null) return null;

            return new AccountResponseDto
            {
                AccountId = account.AccountId,
                FullName = account.FullName,
                Email = account.Email,
                Role = account.Role,
                CreatedDate = account.CreatedDate
            };
        }

        public async Task<(bool Success, string? ErrorMessage, AccountResponseDto? Account)> UpdateAccountAsync(int accountId, UpdateAccountRequestDto dto)
        {
            var account = await _context.SystemAccounts.FindAsync(accountId);
            if (account == null)
            {
                return (false, "Account not found.", null);
            }

            if (!string.IsNullOrWhiteSpace(dto.FullName))
            {
                account.FullName = dto.FullName.Trim();
            }

            if (dto.Role.HasValue)
            {
                account.Role = dto.Role.Value;
            }

            await _context.SaveChangesAsync();

            var response = new AccountResponseDto
            {
                AccountId = account.AccountId,
                FullName = account.FullName,
                Email = account.Email,
                Role = account.Role,
                CreatedDate = account.CreatedDate
            };

            return (true, null, response);
        }

        public async Task<(bool Success, string? ErrorMessage)> DeleteAccountAsync(int accountId)
        {
            var account = await _context.SystemAccounts.FindAsync(accountId);
            if (account == null)
            {
                return (false, "Account not found.");
            }

            // Must be rejected if the account has created any tasks
            var hasCreatedTasks = await _context.Tasks.AnyAsync(t => t.CreatedById == accountId);
            if (hasCreatedTasks)
            {
                return (false, "Cannot delete account: This account has created tasks.");
            }

            _context.SystemAccounts.Remove(account);
            await _context.SaveChangesAsync();

            return (true, null);
        }
    }
}
