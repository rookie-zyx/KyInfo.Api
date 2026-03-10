namespace KyInfo.Api.Dtos;

/// <summary>
/// 注册请求 DTO，用于接收客户端提交的注册信息。
/// 注意：Password 为明文传输（应通过 HTTPS），服务端必须对其进行校验与哈希后存储。
/// </summary>
public class RegisterRequest
{
    /// <summary>
    /// 用户名（用于登录或显示）。应在服务端验证长度、字符集和唯一性。
    /// </summary>
    public string UserName { get; set; } = default!;

    /// <summary>
    /// 邮箱地址。应在服务端验证格式并保证唯一性。
    /// </summary>
    public string Email { get; set; } = default!;

    /// <summary>
    /// 明文密码。切勿在数据库中以明文存储，服务端应使用安全的哈希（例如 PBKDF2 / Argon2 / bcrypt）并加盐。
    /// </summary>
    public string Password { get; set; } = default!;
}

/// <summary>
/// 登录请求 DTO，支持使用用户名或邮箱进行登录。
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// 用户名或邮箱（任一均可用于认证）。
    /// 服务端应根据输入判断并查找对应用户记录。
    /// </summary>
    public string UserNameOrEmail { get; set; } = default!;

    /// <summary>
    /// 明文密码。服务端负责校验哈希后的结果。
    /// </summary>
    public string Password { get; set; } = default!;
}

/// <summary>
/// 认证响应 DTO，向客户端返回认证结果（如 JWT token）及部分用户信息。
/// </summary>
public class AuthResponse
{
    /// <summary>
    /// JWT 访问令牌（应包含过期时间等声明），客户端用于后续受保护接口的授权请求。
    /// </summary>
    public string Token { get; set; } = default!;

    /// <summary>
    /// 已认证的用户名，便于客户端显示。
    /// </summary>
    public string UserName { get; set; } = default!;

    /// <summary>
    /// 用户角色（例如 "User" 或 "Admin"），客户端可据此调整界面或逻辑（但服务器端仍需强制授权检查）。
    /// </summary>
    public string Role { get; set; } = default!;
}
