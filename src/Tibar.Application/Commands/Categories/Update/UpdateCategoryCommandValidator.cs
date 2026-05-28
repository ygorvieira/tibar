using FluentValidation;

namespace Tibar.Application.Commands.Categories.Update;

public class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");

        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Type is required.")
            .Must(t => t.ToLower() is "income" or "expense")
            .WithMessage("Type must be 'income' or 'expense'.");
    }
}
