using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using KyInfo.Application.Services.Admin;
using KyInfo.Contracts.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace KyInfo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Root,Admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminAppService _admin;

    public AdminController(IAdminAppService admin)
    {
        _admin = admin;
    }

    private int? GetCurrentUserId()
    {
        var sub = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                  ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        return sub != null && int.TryParse(sub, out var id) ? id : null;
    }

    [HttpGet("users")]
    public async Task<ActionResult<IReadOnlyList<AdminUserListItemDto>>> GetUsers(CancellationToken cancellationToken)
    {
        return Ok(await _admin.GetUsersAsync(cancellationToken));
    }

    [HttpPost("users")]
    public async Task<IActionResult> CreateUser([FromBody] AdminCreateUserRequest request, CancellationToken cancellationToken)
    {
        var actorId = GetCurrentUserId();
        if (!actorId.HasValue)
        {
            return Unauthorized();
        }

        await _admin.CreateUserAsync(request, actorId.Value, cancellationToken);
        return Ok(new { message = "用户已创建" });
    }

    [HttpDelete("users/{id:int}")]
    public async Task<IActionResult> DeleteUser(int id, CancellationToken cancellationToken)
    {
        var actorId = GetCurrentUserId();
        if (!actorId.HasValue)
        {
            return Unauthorized();
        }

        await _admin.DeleteUserAsync(id, actorId.Value, cancellationToken);
        return Ok(new { message = "用户已删除" });
    }

    [HttpPut("users/{id:int}")]
    public async Task<IActionResult> UpdateUser(
        int id,
        [FromBody] AdminUpdateUserRequest request,
        CancellationToken cancellationToken)
    {
        var actorId = GetCurrentUserId();
        if (!actorId.HasValue)
        {
            return Unauthorized();
        }

        await _admin.UpdateUserAsync(id, actorId.Value, request, cancellationToken);
        return Ok(new { message = "用户已更新" });
    }

    [HttpGet("administrators")]
    [Authorize(Roles = "Root")]
    public async Task<ActionResult<IReadOnlyList<AdminUserListItemDto>>> GetAdministrators(CancellationToken cancellationToken)
    {
        var actorId = GetCurrentUserId();
        if (!actorId.HasValue)
        {
            return Unauthorized();
        }

        return Ok(await _admin.GetAdministratorUsersAsync(actorId.Value, cancellationToken));
    }

    [HttpPost("administrators")]
    [Authorize(Roles = "Root")]
    public async Task<IActionResult> CreateAdministrator(
        [FromBody] AdminCreateAdministratorRequest request,
        CancellationToken cancellationToken)
    {
        var actorId = GetCurrentUserId();
        if (!actorId.HasValue)
        {
            return Unauthorized();
        }

        await _admin.CreateAdministratorAsync(actorId.Value, request, cancellationToken);
        return Ok(new { message = "管理员已创建" });
    }

    [HttpDelete("administrators/{id:int}")]
    [Authorize(Roles = "Root")]
    public async Task<IActionResult> RemoveAdministrator(int id, CancellationToken cancellationToken)
    {
        var actorId = GetCurrentUserId();
        if (!actorId.HasValue)
        {
            return Unauthorized();
        }

        await _admin.RemoveAdministratorAsync(actorId.Value, id, cancellationToken);
        return Ok(new { message = "已撤权管理员（降为普通用户）。" });
    }

    [HttpGet("audit-logs")]
    [Authorize(Roles = "Root")]
    public async Task<ActionResult<AuditLogListResponseDto>> GetAuditLogs(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var actorId = GetCurrentUserId();
        if (!actorId.HasValue)
        {
            return Unauthorized();
        }

        return Ok(await _admin.GetAuditLogsAsync(actorId.Value, page, pageSize, cancellationToken));
    }

    [HttpGet("exam-scores/import-template")]
    public IActionResult GetExamScoreImportTemplate()
    {
        var bytes = _admin.GetExamScoreImportTemplate();
        return File(
            bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "成绩导入模板.xlsx");
    }

    [HttpPost("exam-scores/import")]
    [RequestSizeLimit(32 * 1024 * 1024)]
    [EnableRateLimiting("admin_upload")]
    public async Task<ActionResult<ExcelImportResultDto>> ImportExamScores(
        IFormFile? file,
        CancellationToken cancellationToken)
    {
        var actorId = GetCurrentUserId();
        if (!actorId.HasValue)
        {
            return Unauthorized();
        }

        if (file is null || file.Length == 0)
        {
            return BadRequest(new { message = "请上传 Excel 文件（.xlsx）。" });
        }

        await using var stream = file.OpenReadStream();
        var result = await _admin.ImportExamScoresFromExcelAsync(stream, actorId.Value, cancellationToken);
        return Ok(result);
    }
}
