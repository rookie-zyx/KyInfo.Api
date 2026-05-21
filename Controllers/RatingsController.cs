using KyInfo.Api.Infrastructure;
using KyInfo.Application.Services.Ratings;
using KyInfo.Contracts.Ratings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace KyInfo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RatingsController : ControllerBase
{
    private readonly IRatingAppService _appService;

    public RatingsController(IRatingAppService appService)
    {
        _appService = appService;
    }

    [HttpPost]
    [EnableRateLimiting("ratings_write")]
    public async Task<ActionResult<RatingSubmitResultDto>> Submit(
        [FromBody] RatingSubmitDto dto,
        CancellationToken cancellationToken)
    {
        var userId = JwtUserClaims.GetUserId(User);
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        return Ok(await _appService.SubmitAsync(userId.Value, dto, cancellationToken));
    }

    [HttpGet("{subjectType}/{subjectId:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<RatingDetailDto>> GetDetail(
        string subjectType,
        int subjectId,
        CancellationToken cancellationToken)
    {
        var userId = JwtUserClaims.GetUserId(User);
        return Ok(await _appService.GetDetailAsync(subjectType, subjectId, userId, cancellationToken));
    }
}
