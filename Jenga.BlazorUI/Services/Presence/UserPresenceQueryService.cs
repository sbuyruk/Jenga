using Jenga.DataAccess.Data;
using Jenga.Models.Common;
using Microsoft.EntityFrameworkCore;

namespace Jenga.BlazorUI.Services.Presence
{
    public sealed class UserPresenceQueryService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

        public UserPresenceQueryService(IDbContextFactory<ApplicationDbContext> dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task<List<UserPresenceSession>> GetOpenSessionsAsync(CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);

            return await db.UserPresenceSession_Table
                .AsNoTracking()
                .Where(x => x.DisconnectedAt == null)
                .OrderByDescending(x => x.ConnectedAt)
                .ToListAsync(cancellationToken);
        }

        public sealed record OpenSessionView(
            UserPresenceSession Session,
            string? LastUrl);

        public async Task<List<OpenSessionView>> GetOpenSessionViewsAsync(CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);

            var result = await (
                from s in db.UserPresenceSession_Table.AsNoTracking()
                where s.DisconnectedAt == null
                let lastUrlForSession = (
                    from e in db.UserNavigationEvent_Table.AsNoTracking()
                    where e.PresenceSessionId == s.Id && e.Url != null && e.Url != ""
                    orderby e.OccurredAt descending, e.Id descending
                    select e.Url
                ).FirstOrDefault()
                orderby s.ConnectedAt descending
                select new OpenSessionView(s, lastUrlForSession)
            ).ToListAsync(cancellationToken);

            return result;
        }

        public sealed record UserNavigationHistoryItem(
            DateTime OccurredAtUtc,
            int? PresenceSessionId,
            string Url);

        public async Task<List<UserNavigationHistoryItem>> GetUserNavigationHistoryAsync(
            int personelId,
            int take = 200,
            CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);

            return await db.UserNavigationEvent_Table
                .AsNoTracking()
                .Where(x => x.PersonelId == personelId && x.Url != null && x.Url != "")
                .OrderByDescending(x => x.OccurredAt)
                .ThenByDescending(x => x.Id)
                .Take(take)
                .Select(x => new UserNavigationHistoryItem(
                    x.OccurredAt,
                    x.PresenceSessionId,
                    x.Url!))
                .ToListAsync(cancellationToken);
        }
    }
}
