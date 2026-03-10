using KyInfo.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace KyInfo.Api.Data;

/// <summary>
/// 应用的 EF Core 上下文，封装数据库访问。
/// 在 DI 容器中注册时通常使用 <see cref="DbContextOptions{AppDbContext}"/> 配置连接字符串、提供者等。
/// </summary>
public class AppDbContext : DbContext
{
    /// <summary>
    /// 用指定的选项构造上下文（例如连接字符串、数据库提供者、日志等由外部配置）。
    /// 该构造函数用于在依赖注入中注册 DbContext（例如在 Program.cs 调用 AddDbContext）。
    /// </summary>
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Users 实体集合（对应数据库中的 Users 表）。
    /// 使用 DbSet 可执行 LINQ 查询、添加、更新、删除等操作。
    /// 这里使用表达式属性返回 Set<User>()，避免显式字段/自动属性开销。
    /// </summary>
    public DbSet<User> Users => Set<User>();
    public DbSet<School> Schools => Set<School>();
    public DbSet<Major> Majors => Set<Major>();

    /// <summary>
    /// 自定义模型构建（Fluent API）。
    /// 在此处可以配置表名、索引、约束、字段映射、关系等。
    /// EF Core 在模型第一次被构建时调用该方法。
    /// </summary>
    /// <param name="modelBuilder">模型构建器</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // -----------------------
        // User 实体配置
        // -----------------------
        modelBuilder.Entity<User>(entity =>
        {
            // 在数据库层面为 UserName 与 Email 建立唯一索引，保证唯一性并提升基于这两个字段的查询性能。
            // 注意：同时在业务层验证并处理并发写入时可能出现的唯一约束冲突（捕获 DbUpdateException）。
            entity.HasIndex(u => u.UserName).IsUnique();
            entity.HasIndex(u => u.Email).IsUnique();
        });

        // -----------------------
        // School 实体配置
        // -----------------------
        modelBuilder.Entity<School>(entity =>
        {
            // 为学校名称建立普通索引，提升按名称搜索的性能。
            // 根据需求可改为唯一索引，但许多学校可能存在同名（或简称），因此这里使用非唯一索引。
            entity.HasIndex(s => s.Name);

            // 限制字符串长度以节省存储并匹配业务验证规则。
            // 使用 Fluent API 在数据库层应用长度限制，等同于在迁移中生成相应的列长度。
            entity.Property(s => s.LevelTag).HasMaxLength(50);
            entity.Property(s => s.Type).HasMaxLength(50);
            entity.Property(s => s.Property).HasMaxLength(50);

            // 如需可在此配置默认值、列名、是否必需等，例如：
            // entity.Property(s => s.Province).IsRequired().HasMaxLength(100);
        });

        // -----------------------
        // Major 实体配置
        // -----------------------
        modelBuilder.Entity<Major>(entity =>
        {
            // 为专业代码建立索引（常用于按代码快速查找）。
            entity.HasIndex(m => m.Code);

            // 配置 Major -> School 的一对多关系：
            // - Major 有一个 School（外键 SchoolId）
            // - School 有多个 Majors（导航属性 Majors）
            // - 指定外键为 SchoolId
            // - 设置删除行为为 Cascade：当某个 School 被删除时，其关联的 Major 记录也会被级联删除
            // 注意：级联删除在某些场景可能不希望自动删除数据，可根据业务改为 Restrict/SetNull 等策略。
            entity.HasOne(m => m.School)
                  .WithMany(s => s.Majors)
                  .HasForeignKey(m => m.SchoolId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
