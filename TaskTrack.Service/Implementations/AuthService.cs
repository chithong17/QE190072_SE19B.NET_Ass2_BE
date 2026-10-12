using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using TaskTrack.Repo.Models;
using TaskTrack.Service.DTOs;
using TaskTrack.Service.Interfaces;

namespace TaskTrack.Service.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly TaskmanagementDbEgrzContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(TaskmanagementDbEgrzContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<(bool Success, string? ErrorMessage, AccountResponseDto? Account)> RegisterStaffAsync(RegisterRequestDto dto)
        {
            var normalizedEmail = dto.Email.Trim().ToLowerInvariant();

            var exists = await _context.SystemAccounts
                .AnyAsync(a => a.Email.ToLower() == normalizedEmail);

            if (exists)
            {
                return (false, "Email is already registered.", null);
            }

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password.Trim(), workFactor: 12);

            var account = new SystemAccount
            {
                FullName = dto.FullName.Trim(),
                Email = normalizedEmail,
                PasswordHash = passwordHash,
                Role = 0, // Staff role only via registration
                CreatedDate = DateTime.UtcNow
            };

            await _context.SystemAccounts.AddAsync(account);
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

        public async Task<(bool Success, string? ErrorMessage, AuthResponseDto? Auth)> LoginAsync(LoginRequestDto dto)
        {
            var normalizedEmail = dto.Email.Trim().ToLowerInvariant();

            var account = await _context.SystemAccounts
                .FirstOrDefaultAsync(a => a.Email.ToLower() == normalizedEmail);

            if (account == null)
            {
                return (false, "Invalid email or password.", null);
            }

            bool isPasswordValid = false;
            try
            {
                isPasswordValid = BCrypt.Net.BCrypt.Verify(dto.Password.Trim(), account.PasswordHash);
            }
            catch
            {
                isPasswordValid = false;
            }

            if (!isPasswordValid)
            {
                return (false, "Invalid email or password.", null);
            }

            var token = GenerateJwtToken(account);
            var expiresAt = DateTime.UtcNow.AddHours(24);

            var response = new AuthResponseDto
            {
                Token = token,
                AccountId = account.AccountId,
                FullName = account.FullName,
                Email = account.Email,
                Role = account.Role,
                ExpiresAt = expiresAt
            };

            return (true, null, response);
        }

        public string GenerateJwtToken(SystemAccount account)
        {
            var secret = _configuration["JWT_SECRET"]
                ?? _configuration["Jwt:Secret"]
                ?? "TaskTrackSuperSecretSecurityKey_2026_PRN232_Assignment2_Default!";

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var roleName = account.Role == 1 ? "Admin" : "Staff";
            var roleCode = account.Role.ToString();

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, account.AccountId.ToString()),
                new Claim("AccountId", account.AccountId.ToString()),
                new Claim(ClaimTypes.Name, account.FullName),
                new Claim(ClaimTypes.Email, account.Email),
                new Claim("Email", account.Email),
                new Claim(ClaimTypes.Role, roleName),
                new Claim("Role", roleName),
                new Claim("RoleCode", roleCode),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var expiresAt = DateTime.UtcNow.AddHours(24);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"] ?? "TaskTrackAPI",
                audience: _configuration["Jwt:Audience"] ?? "TaskTrackClient",
                claims: claims,
                expires: expiresAt,
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<(bool Success, string? ErrorMessage)> ChangePasswordAsync(int accountId, ChangePasswordRequestDto dto)
        {
            var account = await _context.SystemAccounts.FindAsync(accountId);
            if (account == null)
            {
                return (false, "Account not found.");
            }

            if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword.Trim(), account.PasswordHash))
            {
                return (false, "Current password is incorrect.");
            }

            account.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword.Trim(), workFactor: 12);
            await _context.SaveChangesAsync();

            return (true, null);
        }
    }
}
