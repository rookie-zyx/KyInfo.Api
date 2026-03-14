using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KyInfo.Contracts.ExamScores;
using KyInfo.Application.Services.ExamScores;

namespace KyInfo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExamScoresController : ControllerBase
{
    private readonly IExamScoreAppService _appService;

    public ExamScoresController(IExamScoreAppService appService)
    {
        _appService = appService;
    }

    // GET: api/examscores?userId=1&year=2025&schoolId=1&majorId=2
    // 示例：该查询接口对所有用户开放（不需要登录）
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ExamScoreListItemDto>>> GetExamScores(
        [FromQuery] int? userId,
        [FromQuery] int? year,
        [FromQuery] int? schoolId,
        [FromQuery] int? majorId,
        CancellationToken cancellationToken = default)
    {
        return await _appService.SearchAsync(userId, year, schoolId, majorId, cancellationToken);
    }

    // GET: api/examscores/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ExamScoreDetailDto>> GetExamScore(int id, CancellationToken cancellationToken = default)
    {
        return await _appService.GetByIdAsync(id, cancellationToken);
    }

    // 后台维护 / 用户录入：新增一次考试成绩
    // 示例：该接口要求用户已登录（需携带 Bearer Token）
    [Authorize]
    [HttpPost]
    public async Task<ActionResult<int>> CreateExamScore(
        [FromBody] ExamScoreCreateDto dto,
        CancellationToken cancellationToken = default)
    {
        return await _appService.CreateAsync(dto, cancellationToken);
    }
}

