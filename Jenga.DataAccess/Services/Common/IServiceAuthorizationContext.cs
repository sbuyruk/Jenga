using Jenga.Models.Enums;

namespace Jenga.DataAccess.Services.Common
{
    /// <summary>
    /// Servis katmanının, çağıranın kim olduğunu ve hangi izinlere sahip olduğunu
    /// sorgulamasına olanak tanıyan soyutlama. BlazorUI tarafında
    /// <c>BlazorServiceAuthorizationContext</c> aracılığıyla <c>ICurrentUserService</c>'e bağlanır.
    /// </summary>
    public interface IServiceAuthorizationContext
    {
        /// <summary>
        /// Mevcut kullanıcının belirtilen modül/operasyon için yetkili olup olmadığını döner.
        /// </summary>
        Task<bool> HasPermissionAsync(ModuleName module, Operation operation, CancellationToken cancellationToken = default);

        /// <summary>
        /// Audit alanlarına yazılmak üzere mevcut kullanıcının adını döner.
        /// </summary>
        Task<string?> GetUserNameAsync(CancellationToken cancellationToken = default);
    }
}
