using FluentValidation;

namespace Tibar.Application.Queries.Accounts;

public class GetAccountsQueryValidator : AbstractValidator<GetAccountsQuery>
{
    public GetAccountsQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("Página deve ser maior ou igual a 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("Tamanho da página deve estar entre 1 e 100.");
    }
}
