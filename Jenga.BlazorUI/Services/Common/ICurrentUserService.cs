using Jenga.Models.Enums;
using Jenga.Models.IKYS;

namespace Jenga.BlazorUI.Services.Common
{
    public interface ICurrentUserService
    {
        /// <summary>
        /// Prerender / OnInitializedAsync gibi JS interop'un henüz hazır olmadığı
        /// aşamalarda çağrılır. Impersonation override'ı atlar.
        /// </summary>
        Task<Personel?> GetCurrentPersonelWithoutImpersonationAsync();

        /// <summary>
        /// Mevcut Blazor circuit'ine ait personeli döner (impersonation dahil).
        /// </summary>
        Task<Personel?> GetCurrentPersonelAsync();

        /// <summary>
        /// Mevcut kullanıcının adını döner (DB sorgusu yapmaz).
        /// Audit alanlarına yazılmak üzere servis katmanına iletilir.
        /// </summary>
        Task<string?> GetUserNameAsync();

        /// <summary>
        /// Önbelleği temizler; bir sonraki çağrıda kullanıcı DB'den yeniden çözümlenir.
        /// </summary>
        void Invalidate();

        /// <summary>
        /// Development-only: belirtilen kullanıcıyı aktif circuit için impersonate eder.
        /// </summary>
        Task<bool> SetImpersonationOverrideAsync(string? overrideUser);

        /// <summary>
        /// Mevcut kullanıcının sahip olduğu tüm modül izinlerini döner.
        /// Sonuç önbelleklenir; Invalidate() ile temizlenebilir.
        /// </summary>
        Task<IReadOnlySet<(ModuleName Module, Operation Operation)>> GetModulePermissionsAsync();

        /// <summary>
        /// Mevcut kullanıcının yetkili olduğu bölge ID'lerini döner.
        /// Boş liste → kullanıcıya henüz bölge atanmamış demektir.
        /// Sonuç önbelleklenir; Invalidate() ile temizlenebilir.
        /// </summary>
        Task<IReadOnlyList<int>> GetAuthorizedRegionIdsAsync();

        /// <summary>
        /// Mevcut kullanıcının belirtilen modül ve operasyon için yetkili olup olmadığını döner.
        /// </summary>
        Task<bool> HasPermissionAsync(ModuleName module, Operation operation);
    }
}
