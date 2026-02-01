using Jenga.BlazorUI.Services.Common;
using Jenga.DataAccess.Data;
using Jenga.Models.Common;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.EntityFrameworkCore;

namespace Jenga.BlazorUI.Services.Presence
{
    public sealed class PresenceCircuitHandler : CircuitHandler
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
        private readonly CurrentUserService _currentUserService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly PresenceHeartbeatState _heartbeatState;

        public PresenceCircuitHandler(
            IDbContextFactory<ApplicationDbContext> dbFactory,
            CurrentUserService currentUserService,
            IHttpContextAccessor httpContextAccessor,
            PresenceHeartbeatState heartbeatState)
        {
            _dbFactory = dbFactory;
            _currentUserService = currentUserService;
            _httpContextAccessor = httpContextAccessor;
            _heartbeatState = heartbeatState;
        }

        public override async Task OnConnectionUpAsync(Circuit circuit, CancellationToken cancellationToken)
        {
            var personel = await _currentUserService.GetCurrentPersonelAsync();
            if (personel == null)
                return;

            var ctx = _httpContextAccessor.HttpContext;
            var userAgent = ctx?.Request.Headers.UserAgent.ToString();
            var remoteIp = ctx?.Connection.RemoteIpAddress?.ToString();

            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);

            var existing = await db.UserPresenceSession_Table
                .FirstOrDefaultAsync(x => x.CircuitId == circuit.Id, cancellationToken);

            if (existing == null)
            {
                var session = new UserPresenceSession
                {
                    PersonelId = personel.Id,
                    UserName = personel.KullaniciAdi,
                    DisplayName = $"{personel.Adi} {personel.Soyadi}",
                    CircuitId = circuit.Id,
                    ConnectedAt = DateTime.Now,
                    LastSeen = DateTime.Now,
                    DisconnectedAt = null,
                    UserAgent = userAgent,
                    RemoteIp = remoteIp,
                    Olusturan = ctx?.User?.Identity?.Name,
                    OlusturmaTarihi = DateTime.Now,
                };

                db.UserPresenceSession_Table.Add(session);
                await db.SaveChangesAsync(cancellationToken);

                _heartbeatState.SessionId = session.Id;
                return;
            }

            existing.PersonelId = personel.Id;
            existing.UserName = personel.KullaniciAdi;
            existing.DisplayName = $"{personel.Adi} {personel.Soyadi}";
            existing.LastSeen = DateTime.Now;
            existing.DisconnectedAt = null;
            existing.UserAgent = userAgent;
            existing.RemoteIp = remoteIp;
            existing.Degistiren = ctx?.User?.Identity?.Name;
            existing.DegistirmeTarihi = DateTime.Now;

            await db.SaveChangesAsync(cancellationToken);

            _heartbeatState.SessionId = existing.Id;
        }

        public override async Task OnConnectionDownAsync(Circuit circuit, CancellationToken cancellationToken)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);

            var session = await db.UserPresenceSession_Table
                .FirstOrDefaultAsync(x => x.CircuitId == circuit.Id, cancellationToken);

            if (session == null)
                return;

            session.LastSeen = DateTime.Now;
            session.DisconnectedAt = DateTime.Now;
            session.DegistirmeTarihi = DateTime.Now;

            await db.SaveChangesAsync(cancellationToken);
        }
    }
}
