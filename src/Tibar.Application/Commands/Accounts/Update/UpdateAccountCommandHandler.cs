using MediatR;
using Tibar.Application.Common;
using Tibar.Application.Interfaces;
using Tibar.Domain.Enums;

namespace Tibar.Application.Commands.Accounts.Update;

public class UpdateAccountCommandHandler(
    IApplicationDbContext context) : IRequestHandler<UpdateAccountCommand, Result<DTOs.AccountDto>>
{
    public async Task<Result<DTOs.AccountDto>> Handle(UpdateAccountCommand request, CancellationToken cancellationToken)
    {
        var account = await context.Accounts
            .FindAsync(new object[] { request.Id }, cancellationToken);

        if (account is null || account.UserId != request.UserId)
            return Result.Failure<DTOs.AccountDto>("Conta não encontrada.");

        var typeResult = ParseType(request.Type);
        if (!typeResult.IsValid)
            return Result.Failure<DTOs.AccountDto>(typeResult.Errors);

        account.Update(request.Description, typeResult.Data!);

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success(new DTOs.AccountDto(
            account.Id,
            account.Description,
            account.Type.ToString(),
            account.CreatedAt));
    }

    private static Result<AccountType> ParseType(string type)
    {
        return type.ToLower() switch
        {
            "checking" => Result.Success(AccountType.Checking),
            "investment" => Result.Success(AccountType.Investment),
            "creditcard" => Result.Success(AccountType.CreditCard),
            _ => Result.Failure<AccountType>("Tipo de conta inválido. Deve ser 'checking', 'investment' ou 'creditcard'.")
        };
    }
}
