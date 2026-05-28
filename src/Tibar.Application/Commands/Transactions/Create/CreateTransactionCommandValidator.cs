using FluentValidation;

namespace Tibar.Application.Commands.Transactions.Create;

public class CreateTransactionCommandValidator : AbstractValidator<CreateTransactionCommand>
{
    public CreateTransactionCommandValidator()
    {
        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than zero.");

        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Type is required.")
            .Must(t => t.ToLower() is "income" or "expense")
            .WithMessage("Type must be 'income' or 'expense'.");

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Category is required.");

        RuleFor(x => x.Date)
            .NotEmpty().WithMessage("Date is required.");
    }
}
