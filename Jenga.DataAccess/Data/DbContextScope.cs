using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Jenga.DataAccess.Data;

/// <summary>
/// <see cref="IDbContextScope"/> varsayılan uygulaması.
/// Bir <see cref="IDbContextFactory{TContext}"/> üzerinden TEK bir context açar,
/// üzerine bir transaction başlatır ve scope süresince ikisini birlikte tutar.
///
/// Önemli: Bu sınıf yalnızca canary (Role akışı) için eklendi. Mevcut repository
/// ve servislerin davranışı değiştirilmemiştir; isteyen iş akışı bu scope'u
/// kendi context'i üzerinden kullanabilir.
/// </summary>
public sealed class DbContextScope : IDbContextScope
{
    private readonly IDbContextTransaction _transaction;
    private bool _committed;
    private bool _disposed;

    public ApplicationDbContext Context { get; }

    private DbContextScope(ApplicationDbContext context, IDbContextTransaction transaction)
    {
        Context = context;
        _transaction = transaction;
    }

    public static async Task<DbContextScope> CreateAsync(
        IDbContextFactory<ApplicationDbContext> dbFactory,
        CancellationToken cancellationToken = default,
        string? currentUser = null)
    {
        ArgumentNullException.ThrowIfNull(dbFactory);

        var ctx = await dbFactory.CreateDbContextAsync(cancellationToken);
        ctx.SetCurrentUser(currentUser);
        try
        {
            var tx = await ctx.Database.BeginTransactionAsync(cancellationToken);
            return new DbContextScope(ctx, tx);
        }
        catch
        {
            await ctx.DisposeAsync();
            throw;
        }
    }

    public async Task<int> CommitAsync(CancellationToken cancellationToken = default)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(DbContextScope));
        if (_committed) throw new InvalidOperationException("Scope already committed.");

        var affected = await Context.SaveChangesAsync(cancellationToken);
        await _transaction.CommitAsync(cancellationToken);
        _committed = true;
        return affected;
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;

        // Commit edilmediyse transaction otomatik rollback olur (Dispose sırasında).
        await _transaction.DisposeAsync();
        await Context.DisposeAsync();
    }
}
