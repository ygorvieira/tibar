using FluentValidation;

namespace Tibar.Application.Commands.Categories.Create;

public class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Nome é obrigatório.")
            .MaximumLength(200).WithMessage("Nome não deve exceder 200 caracteres.");

        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Tipo é obrigatório.")
            .Must(t => t.ToLower() is "income" or "expense")
            .WithMessage("Tipo deve ser 'income' ou 'expense'.");
    }
}
