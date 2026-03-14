using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using KyInfo.Application.Services.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KyInfo.Contracts.Account;

namespace KyInfo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AccountController : ControllerBase
{
    private readonly IAccountAppService _appService;

    public AccountController(IAccountAppService appService)
    {
        _appService = appService;
    }

    private int? GetCurrentUserId()
    {
        var sub = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                  ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        return sub != null && int.TryParse(sub, out var id) ? id : null;
    }

    [HttpGet("me")]
    public async Task<ActionResult<AccountProfileDto>> GetMe(CancellationToken cancellationToken = default)
    {
        var id = GetCurrentUserId();
        if (!id.HasValue)
        {
            return Unauthorized();
        }

        return await _appService.GetMeAsync(id.Value, cancellationToken);
    }

    [HttpPut("profile")]
    public async Task<ActionResult<AccountProfileDto>> UpdateProfile(
        [FromBody] UpdateAccountRequest request,
        CancellationToken cancellationToken = default)
    {
        var id = GetCurrentUserId();
        if (!id.HasValue)
        {
            return Unauthorized();
        }

        return await _appService.UpdateProfileAsync(id.Value, request, cancellationToken);
    }

    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangePasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        var id = GetCurrentUserId();
        if (!id.HasValue)
        {
            return Unauthorized();
        }

        await _appService.ChangePasswordAsync(id.Value, request, cancellationToken);
        return Ok(new { message = "密码已更新" });
    }
}
