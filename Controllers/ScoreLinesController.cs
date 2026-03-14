using KyInfo.Contracts.ScoreLines;
using KyInfo.Application.Services.ScoreLines;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KyInfo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ScoreLinesController : ControllerBase
{
    private readonly IScoreLineAppService _appService;

    public ScoreLinesController(IScoreLineAppService appService)
    {
        _appService = appService;
    }

    // GET: api/scorelines?schoolId=1&majorId=2&year=2025&isNational=false
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ScoreLineListItemDto>>> GetScoreLines(
        [FromQuery] int? schoolId,
        [FromQuery] int? majorId,
        [FromQuery] int? year,
        [FromQuery] bool? isNational,
        CancellationToken cancellationToken = default)
    {
        return await _appService.SearchAsync(schoolId, majorId, year, isNational, cancellationToken);
    }

    // GET: api/scorelines/trend?schoolId=1&majorId=2&isNational=false
    // 返回指定条件下按年份聚合的分数线趋势（用于前端画折线图）
    [HttpGet("trend")]
    public async Task<ActionResult<IEnumerable<ScoreLineTrendPointDto>>> GetTrend(
        [FromQuery] int? schoolId,
        [FromQuery] int? majorId,
        [FromQuery] bool? isNational,
        CancellationToken cancellationToken = default)
    {
        return await _appService.GetTrendAsync(schoolId, majorId, isNational, cancellationToken);
    }

    // GET: api/scorelines/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ScoreLineDetailDto>> GetScoreLine(int id, CancellationToken cancellationToken = default)
    {
        return await _appService.GetByIdAsync(id, cancellationToken);
    }

    // 后台维护用：新增分数线（后续可加权限）
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<int>> CreateScoreLine([FromBody] ScoreLineCreateDto dto, CancellationToken cancellationToken = default)
    {
        return await _appService.CreateAsync(dto, cancellationToken);
    }
}

