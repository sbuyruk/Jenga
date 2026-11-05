using Jenga.Models.TBYS.Kontrol;
using Jenga.Utility.Results;

namespace Jenga.DataAccess.Services.TBYS
{
    /// <summary>
    /// Kontrol formu şablonları ve sürümleri. Yayımlanmış/arşivlenmiş sürümlerin içeriği değişmez;
    /// düzenleme yalnızca taslaklarda yapılır, diğerleri için <see cref="YeniSurumOlusturAsync"/> kullanılır.
    /// </summary>
    public interface ITasinmazKontrolFormService
    {
        Task<Result<List<TasinmazKontrolForm>>> GetAllAsync(bool arsivDahil = false, CancellationToken cancellationToken = default);
        Task<Result<TasinmazKontrolForm>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<Result<List<TasinmazKontrolForm>>> GetSurumlerAsync(string kod, CancellationToken cancellationToken = default);

        /// <summary>Taşınmaz için uygulanabilir yayımlanmış formlar; en özel kapsam (Taşınmaz &gt; Cins &gt; Genel) başta.</summary>
        Task<Result<List<TasinmazKontrolForm>>> GetUygunFormlarAsync(int tasinmazId, CancellationToken cancellationToken = default);

        /// <summary>Yeni kod ile 1. sürüm taslak oluşturur.</summary>
        Task<Result<int>> AddAsync(TasinmazKontrolForm form, CancellationToken cancellationToken = default);

        /// <summary>Yalnızca taslak sürümleri günceller (iyimser eşzamanlılık).</summary>
        Task<Result> UpdateAsync(TasinmazKontrolForm form, CancellationToken cancellationToken = default);

        /// <summary>Yalnızca hiç kontrolde kullanılmamış taslak sürümleri siler.</summary>
        Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);

        /// <summary>Mevcut sürümün kopyasından aynı kodla yeni taslak sürüm üretir.</summary>
        Task<Result<int>> YeniSurumOlusturAsync(int kaynakFormId, CancellationToken cancellationToken = default);

        /// <summary>Mevcut sürümün kopyasından farklı kodla yeni (bağımsız) form üretir.</summary>
        Task<Result<int>> KopyalaAsync(int kaynakFormId, string yeniKod, string yeniAd, CancellationToken cancellationToken = default);

        /// <summary>Taslağı yayımlar; aynı kodun önceki yayımlı sürümünü arşivler.</summary>
        Task<Result> YayimlaAsync(int id, CancellationToken cancellationToken = default);

        Task<Result> ArsivleAsync(int id, CancellationToken cancellationToken = default);
    }
}
