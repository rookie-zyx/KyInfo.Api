using KyInfo.Application.Services.Majors;
using KyInfo.Contracts.Majors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KyInfo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MajorsController : ControllerBase
{
    private readonly IMajorAppService _appService;

    public MajorsController(IMajorAppService appService)
    {
        _appService = appService;
    }

    // GET: api/majors?keyword=计算机&schoolId=1
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MajorListItemDto>>> GetMajors(
        [FromQuery] string? keyword,
        [FromQuery] int? schoolId,
        CancellationToken cancellationToken = default)
    {
        return await _appService.SearchAsync(keyword, schoolId, cancellationToken);
    }

    // GET: api/majors/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<MajorDetailDto>> GetMajor(int id, CancellationToken cancellationToken = default)
    {
        return await _appService.GetByIdAsync(id, cancellationToken);
    }

    // 后台维护用：新增专业
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<int>> CreateMajor([FromBody] MajorDetailDto dto, CancellationToken cancellationToken = default)
    {
        return await _appService.CreateAsync(dto, cancellationToken);
    }
}