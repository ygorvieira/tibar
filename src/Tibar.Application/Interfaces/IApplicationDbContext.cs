using Microsoft.EntityFrameworkCore;
using Tibar.Domain.Entities;

namespace Tibar.Application.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Transaction> Transactions { get; }
    DbSet<Category> Categories { get; }
    DbSet<Account> Accounts { get; }
    DbSet<User> Users { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
