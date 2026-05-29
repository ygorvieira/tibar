using FluentValidation;

namespace Tibar.Application.Commands.Categories.Update;

public class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryCommandValidator()
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
