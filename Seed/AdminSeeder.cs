using KyInfo.Application.Abstractions.Identity;
using KyInfo.Domain.Entities;
using KyInfo.Domain.Enums;
using KyInfo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace KyInfo.Api.Seed;

public static class AdminSeeder
{
    public static async Task TrySeedAsync(IServiceProvider services, IWebHostEnvironment env, CancellationToken cancellationToken = default)
    {
        if (!env.IsDevelopment())
        {
            return;
        }

        using var scope = services.CreateScope();
        var sp = scope.ServiceProvider;

        var options = sp.GetRequiredService<IOptions<AdminSeedOptions>>().Value;
        if (!options.Enabled)
        {
            return;
        }

        var db = sp.GetRequiredService<AppDbContext>();
        var hasher = sp.GetRequiredService<IPasswordHasher>();

        var normalizedUserName = options.UserName.Trim();
        var normalizedEmail = options.Email.Trim();

        if (string.IsNullOrWhiteSpace(normalizedUserName))
        {
            throw new ArgumentException("Seed:Admin:UserName 不能为空");
        }

        if (string.IsNullOrWhiteSpace(normalizedEmail))
        {
            throw new ArgumentException("Seed:Admin:Email 不能为空");
        }

        if (string.IsNullOrWhiteSpace(options.Password))
        {
            throw new ArgumentException("Seed:Admin:Password 不能为空");
        }

        var existing = await db.Users
            .FirstOrDefaultAsync(u => u.UserName == normalizedUserName || u.Email == normalizedEmail, cancellationToken);

        if (existing is null)
        {
            var user = new User
            {
                UserName = normalizedUserName,
                Email = normalizedEmail,
                PasswordHash = hasher.HashPassword(options.Password),
                Role = UserRole.Root,
                CreatedAt = DateTime.UtcNow
            };

            db.Users.Add(user);
            await db.SaveChangesAsync(cancellationToken);
            return;
        }

        // 开发环境下，确保种子 Root 账号始终与配置保持一致：
        // - 角色固定为 Root
        // - 用户名/邮箱与配置同步
        // - 密码重置为配置值
        var changed = false;

        if (!string.Equals(existing.UserName, normalizedUserName, StringComparison.Ordinal))
        {
            existing.UserName = normalizedUserName;
            changed = true;
        }

        if (!string.Equals(existing.Email, normalizedEmail, StringComparison.Ordinal))
        {
            existing.Email = normalizedEmail;
            changed = true;
        }

        if (options.PromoteExistingToRoot && existing.Role != UserRole.Root)
        {
            existing.Role = UserRole.Root;
            changed = true;
        }

        var resetPasswordHash = hasher.HashPassword(options.Password);
        if (!string.Equals(existing.PasswordHash, resetPasswordHash, StringComparison.Ordinal))
        {
            existing.PasswordHash = resetPasswordHash;
            changed = true;
        }

        if (changed)
        {
            await db.SaveChangesAsync(cancellationToken);
        }
    }
}
