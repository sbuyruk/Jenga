using Jenga.Models.Search;
using Jenga.Utility.Results;

namespace Jenga.DataAccess.Services.Search
{
    /// <summary>
    /// Tüm modülleri kapsayan global arama servisi.
    /// Yeni modüller eklendiğinde sadece bu servisin implementasyonu genişletilir.
    /// </summary>
    public interface IGlobalSearchService
    {
        Task<Result<GlobalSearchSonucu>> SearchAsync(string query, CancellationToken cancellationToken = default);
    }
}
