using Jenga.Models.TBYS.Search;
using Jenga.Utility.Results;

namespace Jenga.DataAccess.Services.TBYS
{
    /// <summary>
    /// TBYS'e özgü detay sorgularını barındırır.
    /// Genel arama için IGlobalSearchService kullanılır.
    /// </summary>
    public interface ITbysSearchService
    {
        Task<Result<KiraciDetayVM>> GetKiraciDetayAsync(int kiraciId, CancellationToken cancellationToken = default);
        Task<Result<TasinmazDetayVM>> GetTasinmazDetayAsync(int tasinmazId, CancellationToken cancellationToken = default);
    }
}
