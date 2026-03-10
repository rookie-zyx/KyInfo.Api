using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using KyInfo.Api.Data;
using KyInfo.Api.Dtos;
using KyInfo.Api.Models;
using KyInfo.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace KyInfo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
/// <summary>
/// 认证控制器：处理用户注册与登录，并生成 JWT 令牌。
/// 注意：控制器仅演示基本流程，生产环境请加强输入校验、错误处理与安全措施（详见方法注释）。
/// </summary>
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    /// <summary>
    /// 构造函数，通过 DI 注入 DbContext 与 配置（用于读取 JWT 配置）。
    /// </summary>
    public AuthController(AppDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    /// <summary>
    /// 注册新用户。
    /// - 检查用户名或邮箱是否已存在（数据库唯一性约束已在 DbContext 中定义）。
    /// - 对明文密码进行哈希（此处使用示例 PasswordHasher，生产请使用 PBKDF2/Argon2/bcrypt 等）。
    /// </summary>
    /// <param name="request">包含 UserName、Email、Password 的注册请求 DTO。</param>
    /// <returns>注册结果：成功返回 200，失败返回 400。</returns>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        // 先在业务层快速判断重复，减少后续写入冲突（并发仍需捕获 DbUpdateException）。
        if (await _db.Users.AnyAsync(u => u.UserName == request.UserName || u.Email == request.Email))
        {
            return BadRequest("用户名或邮箱已存在");
        }

        // 创建用户实体并对密码进行哈希存储（示例代码中为 SHA-256，生产请替换为更安全的方案）。
        var user = new User
        {
            UserName = request.UserName,
            Email = request.Email,
            PasswordHash = PasswordHasher.HashPassword(request.Password),
            Role = UserRole.User
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return Ok("注册成功");
    }

    /// <summary>
    /// 登录并返回包含 JWT 的认证响应。
    /// 支持使用用户名或邮箱进行登录。
    /// </summary>
    /// <param name="request">包含 UserNameOrEmail 与 Password 的登录请求 DTO。</param>
    /// <returns>认证成功返回 <see cref="AuthResponse"/>（含 Token），失败返回 401。</returns>
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        // 根据用户名或邮箱查找用户
        var user = await _db.Users
            .FirstOrDefaultAsync(u =>
                u.UserName == request.UserNameOrEmail ||
                u.Email == request.UserNameOrEmail);

        // 校验用户存在并验证密码哈希
        if (user == null || !PasswordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            // 不要在响应中泄露是用户名不存在还是密码错误
            return Unauthorized("用户名或密码错误");
        }

        // 生成 JWT 并返回给客户端
        var token = GenerateJwtToken(user);
        return new AuthResponse
        {
            Token = token,
            UserName = user.UserName,
            Role = user.Role.ToString()
        };
    }

    /// <summary>
    /// 根据用户信息生成 JWT Token。
    /// - 从配置读取 Key/Issuer/Audience/ExpireMinutes。
    /// - 在生产环境必须将密钥保存在机密存储（__dotnet user-secrets__、环境变量或机密管理服务），不要硬编码。
    /// </summary>
    /// <param name="user">用于签发 Token 的用户实体。</param>
    /// <returns>JWT 字符串。</returns>
    private string GenerateJwtToken(User user)
    {
        var jwtSection = _config.GetSection("Jwt");

        // 读取并验证必须的配置项（示例未做完整校验，推荐在启动时验证配置完整性）
        var keyString = jwtSection["Key"] ?? throw new InvalidOperationException("JWT Key 未配置");
        var issuer = jwtSection["Issuer"];
        var audience = jwtSection["Audience"];
        var expireMinutesStr = jwtSection["ExpireMinutes"] ?? "0";

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // 常见声明：sub (用户 id), unique_name (用户名), role (基于 ClaimTypes.Role)
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(int.Parse(expireMinutesStr)),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
