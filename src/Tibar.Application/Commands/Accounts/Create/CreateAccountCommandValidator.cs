using FluentValidation;

namespace Tibar.Application.Commands.Accounts.Create;

public class CreateAccountCommandValidator : AbstractValidator<CreateAccountCommand>
{
    public CreateAccountCommandValidator()
    {
        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Descrição é obrigatória.")
            .MaximumLength(200).WithMessage("Descrição não deve exceder 200 caracteres.");

        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Tipo é obrigatório.")
            .Must(t => t.ToLower() is "checking" or "investment" or "creditcard")
            .WithMessage("Tipo deve ser 'checking', 'investment' ou 'creditcard'.");
    }
}
