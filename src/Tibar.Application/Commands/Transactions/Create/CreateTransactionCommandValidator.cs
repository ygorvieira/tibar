using FluentValidation;

namespace Tibar.Application.Commands.Transactions.Create;

public class CreateTransactionCommandValidator : AbstractValidator<CreateTransactionCommand>
{
    public CreateTransactionCommandValidator()
    {
        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Descrição é obrigatória.")
            .MaximumLength(500).WithMessage("Descrição não deve exceder 500 caracteres.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Valor deve ser maior que zero.");

        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Tipo é obrigatório.")
            .Must(t => t.ToLower() is "income" or "expense")
            .WithMessage("Tipo deve ser 'income' ou 'expense'.");

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Categoria é obrigatória.");

        RuleFor(x => x.AccountId)
            .NotEmpty().WithMessage("Conta é obrigatória.");

        RuleFor(x => x.Date)
            .NotEmpty().WithMessage("Data é obrigatória.");
    }
}
