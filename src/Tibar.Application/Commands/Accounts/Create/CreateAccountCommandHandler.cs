using MediatR;
using Tibar.Application.Common;
using Tibar.Application.Interfaces;
using Tibar.Domain.Entities;
using Tibar.Domain.Enums;

namespace Tibar.Application.Commands.Accounts.Create;

public class CreateAccountCommandHandler(IApplicationDbContext context) : IRequestHandler<CreateAccountCommand, Result<DTOs.AccountDto>>
{
    private readonly IApplicationDbContext _context = context;

    public async Task<Result<DTOs.AccountDto>> Handle(CreateAccountCommand request, CancellationToken cancellationToken)
    {
        var typeResult = ParseType(request.Type);
        if (!typeResult.IsValid)
            return Result.Failure<DTOs.AccountDto>(typeResult.Errors);

        var account = new Account(request.Description, typeResult.Data!, request.UserId);

        _context.Accounts.Add(account);
        await _context.SaveChangesAsync(cancellationToken);

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
