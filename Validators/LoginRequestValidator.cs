using FluentValidation;
using KyInfo.Contracts.Auth;

namespace KyInfo.Api.Validators;

public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.UserNameOrEmail)
            .NotEmpty().WithMessage("用户名或邮箱不能为空。")
            .MaximumLength(256);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("密码不能为空。")
            .MaximumLength(128);
    }
}
