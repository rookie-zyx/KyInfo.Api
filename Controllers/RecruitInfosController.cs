using KyInfo.Application.Services.RecruitInfos;
using Microsoft.AspNetCore.Mvc;
using KyInfo.Contracts.RecruitInfos;
using Microsoft.AspNetCore.Authorization;

namespace KyInfo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RecruitInfosController : ControllerBase
{
    private readonly IRecruitInfoAppService _appService;

    public RecruitInfosController(IRecruitInfoAppService appService)
    {
        _appService = appService;
    }

    // GET: api/recruitinfos?schoolId=1&majorId=2&year=2025
    [HttpGet]
    public async Task<ActionResult<IEnumerable<RecruitInfoListItemDto>>> GetRecruitInfos(
        [FromQuery] int? schoolId,
        [FromQuery] int? majorId,
        [FromQuery] int? year,
        CancellationToken cancellationToken = default)
    {
        return await _appService.SearchAsync(schoolId, majorId, year, cancellationToken);
    }

    // GET: api/recruitinfos/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<RecruitInfoDetailDto>> GetRecruitInfo(int id, CancellationToken cancellationToken = default)
    {
        return await _appService.GetByIdAsync(id, cancellationToken);
    }

    // 后台维护用：新增报考信息 / 招生简章
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<int>> CreateRecruitInfo([FromBody] RecruitInfoCreateDto dto, CancellationToken cancellationToken = default)
    {
        return await _appService.CreateAsync(dto, cancellationToken);
    }
}

