using FluentValidation;

namespace Tibar.Application.Commands.Auth.Login;

public class LoginUserCommandValidator : AbstractValidator<LoginUserCommand>
{
    public LoginUserCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-mail é obrigatório.")
            .EmailAddress().WithMessage("Formato de e-mail inválido.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Senha é obrigatória.");
    }
}
