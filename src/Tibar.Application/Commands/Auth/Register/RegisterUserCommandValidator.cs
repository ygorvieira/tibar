using FluentValidation;

namespace Tibar.Application.Commands.Auth.Register;

public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Nome é obrigatório.")
            .MaximumLength(200).WithMessage("Nome não deve exceder 200 caracteres.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-mail é obrigatório.")
            .EmailAddress().WithMessage("Formato de e-mail inválido.")
            .MaximumLength(200).WithMessage("E-mail não deve exceder 200 caracteres.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Senha é obrigatória.")
            .MinimumLength(6).WithMessage("Senha deve ter pelo menos 6 caracteres.")
            .MaximumLength(100).WithMessage("Senha não deve exceder 100 caracteres.");
    }
}
