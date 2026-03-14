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
                Role = UserRole.Admin,
                CreatedAt = DateTime.UtcNow
            };

            db.Users.Add(user);
            await db.SaveChangesAsync(cancellationToken);
            return;
        }

        if (options.PromoteExistingToAdmin && existing.Role != UserRole.Admin)
        {
            existing.Role = UserRole.Admin;
            await db.SaveChangesAsync(cancellationToken);
        }
    }
}

