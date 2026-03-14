using KyInfo.Application.Services.Schools;
using KyInfo.Contracts.Schools;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KyInfo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SchoolsController : ControllerBase
{
    private readonly ISchoolAppService _appService;

    public SchoolsController(ISchoolAppService appService)
    {
        _appService = appService;
    }

    // GET: api/schools?keyword=工大&province=北京&levelTag=985
    [HttpGet]
    public async Task<ActionResult<IEnumerable<SchoolListItemDto>>> GetSchools(
        [FromQuery] string? keyword,
        [FromQuery] string? province,
        [FromQuery] string? levelTag,
        CancellationToken cancellationToken)
    {
        var result = await _appService.SearchAsync(keyword, province, levelTag, cancellationToken);
        return result;
    }

    // GET: api/schools/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<SchoolDetailDto>> GetSchool(int id, CancellationToken cancellationToken)
    {
        return await _appService.GetByIdAsync(id, cancellationToken);
    }

    // 后台维护用：新增院校（后续可加权限）
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<int>> CreateSchool([FromBody] SchoolDetailDto dto, CancellationToken cancellationToken)
    {
        var id = await _appService.CreateAsync(dto, cancellationToken);
        return id;
    }
}
