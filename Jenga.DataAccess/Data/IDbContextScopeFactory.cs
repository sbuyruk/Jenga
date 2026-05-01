namespace Jenga.DataAccess.Data;

/// <summary>
/// Birden fazla tabloya yazan iş akışları için tek bir
/// <see cref="ApplicationDbContext"/> + transaction sarmalayıcısı (<see cref="IDbContextScope"/>) üretir.
///
/// Servisler doğrudan static <c>DbContextScope.CreateAsync(...)</c> çağırmak yerine
/// bu factory'yi enjekte etmelidir; böylece scope üretimi mock'lanabilir ve test edilebilir hâle gelir.
///
/// Tipik kullanım (yeni bir use-case servisinde):
/// <code>
/// public class MyUseCaseService
/// {
///     private readonly IDbContextScopeFactory _scopeFactory;
///     public MyUseCaseService(IDbContextScopeFactory scopeFactory) =&gt; _scopeFactory = scopeFactory;
///
///     public async Task DoAsync(CancellationToken ct)
///     {
///         await using var scope = await _scopeFactory.CreateAsync(ct);
///         var db = scope.Context;
///
///         // db.Set&lt;A&gt;().Add(...); db.Set&lt;B&gt;().Add(...);  (ara SaveChanges gerekirse identity için, hâlâ aynı transaction)
///
///         await scope.CommitAsync(ct);  // tek atomik commit; hata olursa using sonu rollback
///     }
/// }
/// </code>
/// </summary>
public interface IDbContextScopeFactory
{
    /// <summary>
    /// Yeni bir <see cref="IDbContextScope"/> başlatır: tek context açılır ve üzerinde transaction başlatılır.
    /// </summary>
    Task<IDbContextScope> CreateAsync(CancellationToken cancellationToken = default);
}
