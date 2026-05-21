using Jenga.DataAccess.Services.Common;
using Jenga.Models.Enums;

namespace Jenga.BlazorUI.Services.Common
{
    /// <summary>
    /// <see cref="IServiceAuthorizationContext"/>'in Blazor UI implementasyonu.
    /// Mevcut <see cref="ICurrentUserService"/> üzerinden yetki ve kullanıcı bilgisini sağlar.
    /// </summary>
    public sealed class BlazorServiceAuthorizationContext : IServiceAuthorizationContext
    {
        private readonly ICurrentUserService _currentUserService;

        public BlazorServiceAuthorizationContext(ICurrentUserService currentUserService)
        {
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        }

        public async Task<bool> HasPermissionAsync(ModuleName module, Operation operation, CancellationToken cancellationToken = default)
            => await _currentUserService.HasPermissionAsync(module, operation);

        public async Task<string?> GetUserNameAsync(CancellationToken cancellationToken = default)
            => await _currentUserService.GetUserNameAsync();
    }
}
