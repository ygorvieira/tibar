using FluentValidation;

namespace Tibar.Application.Queries.Transactions;

public class GetTransactionsByPeriodQueryValidator : AbstractValidator<GetTransactionsByPeriodQuery>
{
    public GetTransactionsByPeriodQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("Página deve ser maior ou igual a 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("Tamanho da página deve estar entre 1 e 100.");

        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate).WithMessage("Data final deve ser maior ou igual à data inicial.");
    }
}
