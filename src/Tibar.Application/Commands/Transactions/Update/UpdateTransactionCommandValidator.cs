using FluentValidation;

namespace Tibar.Application.Commands.Transactions.Update;

public class UpdateTransactionCommandValidator : AbstractValidator<UpdateTransactionCommand>
{
    public UpdateTransactionCommandValidator()
    {
        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Descrição é obrigatória.")
            .MaximumLength(500).WithMessage("Descrição não deve exceder 500 caracteres.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Valor deve ser maior que zero.");

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Categoria é obrigatória.");

        RuleFor(x => x.Date)
            .NotEmpty().WithMessage("Data é obrigatória.");
    }
}
