using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaskTrack.Service.DTOs;
using TaskTrack.Service.Interfaces;

namespace TaskTrack.API.Controllers
{
    [Route("api/[controller]")]
    [Route("[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IAccountService _accountService;

        public AuthController(IAuthService authService, IAccountService accountService)
        {
            _authService = authService;
            _accountService = accountService;
        }

        /// <summary>
        /// Register a new Staff account (Public - no token required)
        /// </summary>
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.RegisterStaffAsync(dto);
            if (!result.Success)
            {
                return StatusCode(StatusCodes.Status409Conflict, new { message = result.ErrorMessage });
            }

            return StatusCode(StatusCodes.Status201Created, result.Account);
        }

        /// <summary>
        /// Authenticate user credentials and return signed JWT token (Public - no token required)
        /// </summary>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.LoginAsync(dto);
            if (!result.Success)
            {
                return StatusCode(StatusCodes.Status401Unauthorized, new { message = result.ErrorMessage });
            }

            return Ok(result.Auth);
        }

        /// <summary>
        /// Get currently logged-in account profile (Authenticated)
        /// </summary>
        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetCurrentProfile()
        {
            var accountIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("AccountId");
            if (!int.TryParse(accountIdClaim, out var accountId))
            {
                return Unauthorized();
            }

            var account = await _accountService.GetAccountByIdAsync(accountId);
            if (account == null)
            {
                return NotFound(new { message = "Account not found." });
            }

            return Ok(account);
        }

        /// <summary>
        /// Change password for currently logged-in user (Authenticated)
        /// </summary>
        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var accountIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("AccountId");
            if (!int.TryParse(accountIdClaim, out var accountId))
            {
                return Unauthorized();
            }

            var result = await _authService.ChangePasswordAsync(accountId, dto);
            if (!result.Success)
            {
                return BadRequest(new { message = result.ErrorMessage });
            }

            return Ok(new { message = "Password changed successfully." });
        }

        /// <summary>
        /// Refresh access token using a valid refresh token (Public - no token required)
        /// </summary>
        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.RefreshTokenAsync(dto);
            if (!result.Success)
            {
                return Unauthorized(new { message = result.ErrorMessage });
            }

            return Ok(result.Auth);
        }

        /// <summary>
        /// Revoke the active refresh token for the logged-in user (Authenticated)
        /// </summary>
        [HttpPost("revoke")]
        [Authorize]
        public async Task<IActionResult> Revoke()
        {
            var accountIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("AccountId");
            if (int.TryParse(accountIdClaim, out var accountId))
            {
                await _authService.RevokeRefreshTokenAsync(accountId);
            }

            return Ok(new { message = "Refresh token revoked successfully." });
        }
    }
}
