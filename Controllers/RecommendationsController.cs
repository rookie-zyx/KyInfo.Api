using KyInfo.Application.Services.Recommendations;
using KyInfo.Contracts.Recommendations;
using Microsoft.AspNetCore.Mvc;

namespace KyInfo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
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
        var request = new RecommendationRequestDto
        {
            UserId = userId,
            Year = year,
            Top = top
        };

        return await _appService.GetRecommendationsAsync(request, cancellationToken);
    }
}

