using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Data;

/// <summary>
/// Tek bir <see cref="ApplicationDbContext"/> ve onun üstünde açılmış bir transaction'ı
/// birlikte tutan, use-case (iş akışı) ömürlü bir sarmalayıcı.
///
/// Birden fazla tabloya yazan iş akışlarının tek bir atomic SaveChanges + Commit
/// içinde çalışmasını sağlamak için kullanılır.
///
/// Kullanım deseni:
///     await using var scope = await DbContextScope.CreateAsync(_dbFactory, ct);
///     // scope.Context üzerinde Add / Update / Remove yapılır (SaveChanges YOK)
///     await scope.CommitAsync(ct);   // hata olursa using sonu Rollback eder
/// </summary>
public interface IDbContextScope : IAsyncDisposable
{
    /// <summary>Bu scope'a ait paylaşılan context.</summary>
    ApplicationDbContext Context { get; }

    /// <summary>SaveChanges + transaction Commit. Yalnızca bir kez çağrılmalıdır.</summary>
    Task<int> CommitAsync(CancellationToken cancellationToken = default);
}
