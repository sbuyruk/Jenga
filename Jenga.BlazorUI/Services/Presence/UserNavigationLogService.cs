using Jenga.BlazorUI.Services.Common;
using Jenga.DataAccess.Data;
using Jenga.Models.Common;
using Microsoft.EntityFrameworkCore;

namespace Jenga.BlazorUI.Services.Presence
{
    public sealed class UserNavigationLogService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
        private readonly ICurrentUserService _currentUserService;
        private readonly PresenceHeartbeatState _presenceState;

        public UserNavigationLogService(
            IDbContextFactory<ApplicationDbContext> dbFactory,
            ICurrentUserService currentUserService,
            PresenceHeartbeatState presenceState)
        {
            _dbFactory = dbFactory;
            _currentUserService = currentUserService;
            _presenceState = presenceState;
        }

        public async Task LogAsync(string url, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(url))
                return;

            var personel = await _currentUserService.GetCurrentPersonelAsync();
            if (personel == null)
                return;

            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);

            db.UserNavigationEvent_Table.Add(new UserNavigationEvent
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
