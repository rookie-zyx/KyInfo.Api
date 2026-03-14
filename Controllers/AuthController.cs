using KyInfo.Contracts.Auth;
using KyInfo.Application.Services.Auth;
using Microsoft.AspNetCore.Mvc;

namespace KyInfo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
/// <summary>
/// 认证控制器：处理用户注册与登录，并生成 JWT 令牌。
/// 注意：控制器仅演示基本流程，生产环境请加强输入校验、错误处理与安全措施（详见方法注释）。
/// </summary>
public class AuthController : ControllerBase
{
    private readonly IAuthAppService _appService;

    /// <summary>
    /// 构造函数，通过 DI 注入应用服务。
    /// </summary>
    public AuthController(IAuthAppService appService)
    {
        _appService = appService;
    }

    /// <summary>
    /// 注册新用户。
    /// - 检查用户名或邮箱是否已存在（数据库唯一性约束已在 DbContext 中定义）。
    /// - 对明文密码进行哈希（此处使用示例 PasswordHasher，生产请使用 PBKDF2/Argon2/bcrypt 等）。
    /// </summary>
    /// <param name="request">包含 UserName、Email、Password 的注册请求 DTO。</param>
    /// <returns>注册结果：成功返回 200，失败返回 400。</returns>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        await _appService.RegisterAsync(request, cancellationToken);
        return Ok(new { message = "注册成功" });
    }

    /// <summary>
    /// 登录并返回包含 JWT 的认证响应。
    /// 支持使用用户名或邮箱进行登录。
    /// </summary>
    /// <param name="request">包含 UserNameOrEmail 与 Password 的登录请求 DTO。</param>
    /// <returns>认证成功返回 <see cref="AuthResponse"/>（含 Token），失败返回 401。</returns>
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var auth = await _appService.LoginAsync(request, cancellationToken);
        if (auth is null)
        {
            return Unauthorized(new { message = "用户名或密码错误" });
        }

        return auth;
    }
}
