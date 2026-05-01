using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Data;

/// <summary>
/// <see cref="IDbContextScopeFactory"/> varsayılan uygulaması.
/// Stateless; DI'da Singleton olarak kayıt edilebilir.
/// </summary>
public sealed class DbContextScopeFactory : IDbContextScopeFactory
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

    public DbContextScopeFactory(IDbContextFactory<ApplicationDbContext> dbFactory)
    {
        _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
    }

    public async Task<IDbContextScope> CreateAsync(CancellationToken cancellationToken = default)
        => await DbContextScope.CreateAsync(_dbFactory, cancellationToken);
}
