using Jenga.BlazorUI.Services.Common;
using Jenga.DataAccess.Data;
using Jenga.Models.Common;
using Microsoft.EntityFrameworkCore;

namespace Jenga.BlazorUI.Services.Presence
{
    public sealed class UserNavigationLogService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
        private readonly CurrentUserService _currentUserService;
        private readonly PresenceHeartbeatState _presenceState;

        public UserNavigationLogService(
            IDbContextFactory<ApplicationDbContext> dbFactory,
            CurrentUserService currentUserService,
            PresenceHeartbeatState presenceState)
        {
            _dbFactory = dbFactory;
            _currentUserService = currentUserService;
            _presenceState = presenceState;
        }

        public async Task LogAsync(string url, CancellationToken cancellationToken = default)
        {
            var personel = await _currentUserService.GetCurrentPersonelAsync();
            if (personel == null)
                return;

            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);

            db.Set<UserNavigationEvent>().Add(new UserNavigationEvent
            {
                PersonelId = personel.Id,
                PresenceSessionId = _presenceState.SessionId,
                Url = url,
                OccurredAt = DateTime.Now,
                Olusturan = personel.KullaniciAdi,
                OlusturmaTarihi = DateTime.Now,
            });

            await db.SaveChangesAsync(cancellationToken);
        }
    }
}
