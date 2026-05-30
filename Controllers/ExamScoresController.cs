using KyInfo.Api.Infrastructure;
using KyInfo.Application.Services.ExamScores;
using KyInfo.Contracts.ExamScores;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KyInfo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ExamScoresController : ControllerBase
{
    private readonly IExamScoreAppService _appService;

    public ExamScoresController(IExamScoreAppService appService)
    {
        _appService = appService;
    }

    // GET: api/examscores?userId=1&year=2025&schoolId=1&majorId=2
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ExamScoreListItemDto>>> GetExamScores(
        [FromQuery] int? userId,
        [FromQuery] int? year,
        [FromQuery] int? schoolId,
        [FromQuery] int? majorId,
        CancellationToken cancellationToken = default)
    {
        var actorId = JwtUserClaims.GetUserId(User);
        if (!actorId.HasValue)
        {
            return Unauthorized();
        }

        var role = JwtUserClaims.GetRole(User);
        if (!JwtUserClaims.IsStaff(role))
        {
            if (userId.HasValue && userId.Value != actorId.Value)
            {
                return Forbid();
            }

            userId = actorId;
        }

        return await _appService.SearchAsync(userId, year, schoolId, majorId, cancellationToken);
    }

    // GET: api/examscores/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ExamScoreDetailDto>> GetExamScore(int id, CancellationToken cancellationToken = default)
    {
        var actorId = JwtUserClaims.GetUserId(User);
        if (!actorId.HasValue)
        {
            return Unauthorized();
        }

        var detail = await _appService.GetByIdAsync(id, cancellationToken);
        var role = JwtUserClaims.GetRole(User);
        if (!JwtUserClaims.IsStaff(role) && detail.UserId != actorId.Value)
        {
            return Forbid();
        }

        return detail;
    }

    [HttpPost]
    public async Task<ActionResult<int>> CreateExamScore(
        [FromBody] ExamScoreCreateDto dto,
        CancellationToken cancellationToken = default)
    {
        var actorId = JwtUserClaims.GetUserId(User);
        if (!actorId.HasValue)
        {
            return Unauthorized();
        }

        var role = JwtUserClaims.GetRole(User);
        var id = await _appService.CreateAsync(dto, actorId.Value, role, cancellationToken);
        return Ok(id);
    }
}
