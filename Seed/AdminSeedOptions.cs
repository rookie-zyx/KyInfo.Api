namespace KyInfo.Api.Seed;

public sealed class AdminSeedOptions
{
    public const string SectionName = "Seed:Admin";

    public bool Enabled { get; set; }

    public string UserName { get; set; } = "admin";

    public string Email { get; set; } = "admin@local";

    public string Password { get; set; } = "Admin123!";

    /// <summary>
    /// 若种子用户名/邮箱已存在但角色不是 Root，是否将其提升为 Root（开发环境）。
    /// </summary>
    public bool PromoteExistingToRoot { get; set; } = true;
}

