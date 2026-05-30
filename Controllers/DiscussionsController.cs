using KyInfo.Api.Infrastructure;
using KyInfo.Application.Services.Discussions;
using KyInfo.Contracts.Discussions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace KyInfo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DiscussionsController : ControllerBase
{
    private readonly IDiscussionAppService _appService;

    public DiscussionsController(IDiscussionAppService appService)
    {
        _appService = appService;
    }

    private int? GetCurrentUserId() => JwtUserClaims.GetUserId(User);

    private string GetCurrentUserRole() => JwtUserClaims.GetRole(User);

    [HttpGet("stickers")]
    [AllowAnonymous]
    public ActionResult<IReadOnlyList<StickerDto>> GetStickers()
    {
        return Ok(_appService.GetStickers());
    }

    [HttpGet]
    public async Task<ActionResult<DiscussionListResponseDto>> GetDiscussions(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? keyword = null,
        [FromQuery] string? sort = null,
        [FromQuery] int? authorUserId = null,
        [FromQuery] string? zone = null,
        CancellationToken cancellationToken = default)
    {
        bool? isTeacherZone = null;
        if (string.Equals(zone, DiscussionZoneOptions.Teacher, StringComparison.OrdinalIgnoreCase))
        {
            isTeacherZone = true;
        }
        else if (string.Equals(zone, DiscussionZoneOptions.Student, StringComparison.OrdinalIgnoreCase))
        {
            isTeacherZone = false;
        }
        return Ok(await _appService.SearchAsync(page, pageSize, keyword, sort, authorUserId, isTeacherZone, cancellationToken));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<DiscussionDetailDto>> GetDiscussion(int id, CancellationToken cancellationToken = default)
    {
        return Ok(await _appService.GetByIdAsync(id, cancellationToken));
    }

    [HttpPost]
    [EnableRateLimiting("discussions_write")]
    public async Task<ActionResult<int>> CreateDiscussion(
        [FromBody] DiscussionCreateDto dto,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        var id = await _appService.CreateDiscussionAsync(dto, userId.Value, cancellationToken);
        return Ok(id);
    }

    [HttpPost("{id:int}/comments")]
    [EnableRateLimiting("discussions_write")]
    public async Task<ActionResult<int>> CreateComment(
        int id,
        [FromBody] DiscussionCommentCreateDto dto,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        var commentId = await _appService.CreateCommentAsync(id, dto, userId.Value, cancellationToken);
        return Ok(commentId);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteDiscussion(int id, CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        await _appService.DeleteDiscussionAsync(id, userId.Value, GetCurrentUserRole(), cancellationToken);
        return Ok(new { message = "讨论帖已删除" });
    }

    [HttpDelete("comments/{commentId:int}")]
    public async Task<IActionResult> DeleteComment(int commentId, CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        await _appService.DeleteCommentAsync(commentId, userId.Value, GetCurrentUserRole(), cancellationToken);
        return Ok(new { message = "评论已删除" });
    }
}
