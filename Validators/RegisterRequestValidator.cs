using FluentValidation;
using KyInfo.Contracts.Auth;

namespace KyInfo.Api.Validators;

public sealed class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage("用户名不能为空。")
            .Length(2, 64).WithMessage("用户名长度应在 2～64 个字符之间。");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("邮箱不能为空。")
            .EmailAddress().WithMessage("邮箱格式不正确。")
            .MaximumLength(256);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("密码不能为空。")
            .MinimumLength(6).WithMessage("密码长度至少 6 位。")
            .MaximumLength(128);
    }
}
