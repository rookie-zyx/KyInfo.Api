using System.Security.Cryptography;
using System.Text;

namespace KyInfo.Api.Services;

/// <summary>
/// 简单的密码哈希帮助类（使用 SHA‑256 并以 Base64 表示）。
/// <para>
/// 注意：SHA‑256 不是为密码存储设计的（它速度快、抗暴力破解能力弱）。
/// 不建议在生产环境使用本实现来存储用户密码。应使用专为密码哈希设计的算法，例如 PBKDF2（Rfc2898DeriveBytes）、bcrypt、scrypt 或 Argon2，
/// 或直接使用 ASP.NET Core Identity/现成库的实现以获得更好的安全性和参数管理能力。
/// </para>
/// </summary>
public static class PasswordHasher
{
    /// <summary>
    /// 使用 SHA‑256 对传入的明文密码进行哈希并返回 Base64 编码的结果。
    /// </summary>
    /// <param name="password">明文密码（调用方必须确保通过 HTTPS 传输）。</param>
    /// <returns>Base64 编码的哈希值字符串。</returns>
    public static string HashPassword(string password)
    {
        // 使用 SHA256 对密码字节进行一次性哈希（无盐、无迭代）。
        // 该实现简单且不可逆，但不具备抗暴力／抗硬件加速攻击的特性。
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }

    /// <summary>
    /// 验证明文密码是否与存储的哈希匹配。
    /// </summary>
    /// <param name="password">待验证的明文密码。</param>
    /// <param name="hash">存储的 Base64 哈希字符串。</param>
    /// <returns>如果匹配则返回 true，否则返回 false。</returns>
    /// <remarks>
    /// 目前使用简单的字符串比较。为防止定时攻击，建议使用
    /// <see cref="CryptographicOperations.FixedTimeEquals(byte[], byte[])"/> 对比字节数组。
    /// 例如：
    /// <c>
    /// var a = Convert.FromBase64String(hash);
    /// var b = Convert.FromBase64String(HashPassword(password));
    /// return CryptographicOperations.FixedTimeEquals(a, b);
    /// </c>
    /// </remarks>
    public static bool VerifyPassword(string password, string hash)
    {
        // 这里直接比较两个 Base64 字符串（注意：这不是抗定时攻击的比较）。
        return HashPassword(password) == hash;
    }
}