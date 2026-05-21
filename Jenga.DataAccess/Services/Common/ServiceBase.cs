using Jenga.Models.Enums;
using Jenga.Utility.Results;

namespace Jenga.DataAccess.Services.Common
{
    /// <summary>
    /// Tüm DataAccess servislerinin kalıtım alacağı temel sınıf.
    /// <para>
    /// <c>IServiceAuthorizationContext</c> opsiyonel olarak enjekte edilebilir;
    /// enjekte edilmezse yetki kontrolü atlanır (ör. arka plan işleri, test ortamı).
    /// </para>
    /// </summary>
    public abstract class ServiceBase
    {
        private readonly IServiceAuthorizationContext? _authContext;

        protected ServiceBase(IServiceAuthorizationContext? authContext = null)
        {
            _authContext = authContext;
        }

        /// <summary>
        /// Mevcut kullanıcının belirtilen operasyon için yetkili olup olmadığını kontrol eder.
        /// Yetkisiz ise <c>Result.Failure</c> içeren bir <see cref="Result"/> döner;
        /// yetkili veya auth context yoksa <c>null</c> döner (işleme devam edilebilir).
        /// </summary>
        protected async Task<Result?> CheckAsync(
            ModuleName module,
            Operation operation,
            CancellationToken cancellationToken = default)
        {
            if (_authContext is null)
                return null;

            var allowed = await _authContext.HasPermissionAsync(module, operation, cancellationToken);
            if (!allowed)
                return Result.Failure(Error.Forbidden(
                    $"Bu işlem için gerekli yetki bulunamadı ({module}/{operation}).",
                    $"{module}.{operation}.Forbidden"));

            return null;
        }

        /// <summary>
        /// Mevcut kullanıcının adını döner (audit için).
        /// </summary>
        protected Task<string?> GetUserNameAsync(CancellationToken cancellationToken = default)
            => _authContext is not null
                ? _authContext.GetUserNameAsync(cancellationToken)
                : Task.FromResult<string?>(null);
    }
}
