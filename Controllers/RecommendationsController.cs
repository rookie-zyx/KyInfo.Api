using KyInfo.Api.Infrastructure;
using KyInfo.Application.Services.Recommendations;
using KyInfo.Contracts.Recommendations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KyInfo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RecommendationsController : ControllerBase
{
    private readonly IRecommendationAppService _appService;

    public RecommendationsController(IRecommendationAppService appService)
    {
        _appService = appService;
    }

    // GET: api/recommendations?userId=1&year=2025&top=30
    [HttpGet]
    public async Task<ActionResult<RecommendationResponseDto>> GetRecommendations(
        [FromQuery] int userId,
        [FromQuery] int? year,
        [FromQuery] int top = 30,
        CancellationToken cancellationToken = default)
    {
        var actorId = JwtUserClaims.GetUserId(User);
        if (!actorId.HasValue)
        {
            return Unauthorized();
        }

        var role = JwtUserClaims.GetRole(User);
        if (!JwtUserClaims.IsStaff(role) && userId != actorId.Value)
        {
            return Forbid();
        }

        var request = new RecommendationRequestDto
        {
            UserId = userId,
            Year = year,
            Top = top
        };

        return await _appService.GetRecommendationsAsync(request, cancellationToken);
    }
}
