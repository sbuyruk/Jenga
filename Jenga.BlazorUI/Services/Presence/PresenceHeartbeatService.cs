using Jenga.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace Jenga.BlazorUI.Services.Presence
{
    public sealed class PresenceHeartbeatService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
        private readonly PresenceHeartbeatState _state;

        public PresenceHeartbeatService(IDbContextFactory<ApplicationDbContext> dbFactory, PresenceHeartbeatState state)
        {
            _dbFactory = dbFactory;
            _state = state;
        }

        public async Task TouchAsync(CancellationToken cancellationToken = default)
        {
            if (_state.SessionId is null)
                return;

            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);

            var session = await db.UserPresenceSession_Table
                .FirstOrDefaultAsync(x => x.Id == _state.SessionId.Value, cancellationToken);

            if (session == null)
                return;

            session.LastSeen = DateTime.Now;
            session.DegistirmeTarihi = DateTime.Now;

            await db.SaveChangesAsync(cancellationToken);
        }
    }
}
