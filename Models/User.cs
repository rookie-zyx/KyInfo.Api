namespace KyInfo.Api.Models
{
    public enum UserRole
    {
        User = 0,
        Admin = 1
    }

    public class User
    {
        public int Id { get; set; }

        public string UserName { get; set; } = default!;

        public string Email { get; set; } = default!;

        public string PasswordHash { get; set; } = default!;

        /// <summary>
        /// 用户角色，用于授权判断。
        /// 默认值为 <see cref="UserRole.User"/>（普通用户）。
        /// </summary>
        public UserRole Role { get; set; } = UserRole.User;

        /// <summary>
        /// 账号创建时间（UTC）。
        /// 在实例创建时默认设置为 <see cref="DateTime.UtcNow"/>。
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
