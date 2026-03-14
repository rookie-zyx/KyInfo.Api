namespace KyInfo.Api.Seed;

public sealed class AdminSeedOptions
{
    public const string SectionName = "Seed:Admin";

    public bool Enabled { get; set; }

    public string UserName { get; set; } = "admin";

    public string Email { get; set; } = "admin@local";

    public string Password { get; set; } = "Admin123!";

    public bool PromoteExistingToAdmin { get; set; } = true;
}

