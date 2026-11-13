using Jenga.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace Jenga.BlazorUI.Services.Presence;

public sealed class PresenceHeartbeatService(
    IDbContextFactory<ApplicationDbContext> dbFactory,
    PresenceHeartbeatState state,
    TimeProvider clock,
    ILogger<PresenceHeartbeatService> logger) : IAsyncDisposable
{
    public static readonly TimeSpan Interval = TimeSpan.FromSeconds(30);
    public static readonly TimeSpan OnlineTimeout = TimeSpan.FromSeconds(90);

    private CancellationTokenSource? _cts;
    private Task? _loop;

    public async Task StartAsync()
    {
        await StopAsync();
        if (state.SessionId is not int sessionId)
            return;

        _cts = new CancellationTokenSource();
        _loop = RunAsync(sessionId, _cts.Token);
    }

    private async Task RunAsync(int sessionId, CancellationToken token)
    {
        using var timer = new PeriodicTimer(Interval, clock);
        try
        {
            while (await timer.WaitForNextTickAsync(token))
            {
                try
                {
                    await TouchAsync(sessionId, token);
                }
                catch (OperationCanceledException) when (token.IsCancellationRequested) { break; }
                catch (Exception ex)
                {
                    // Retry on the next tick after a transient database failure.
                    logger.LogWarning(ex, "Presence heartbeat failed for session {SessionId}", sessionId);
                }
            }
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested) { }
    }

    public async Task TouchAsync(int sessionId, CancellationToken cancellationToken = default)
    {
        var now = clock.GetLocalNow().DateTime;
        await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);

        // Never reopen a disconnected session, including a heartbeat racing with disconnect.
        await db.UserPresenceSession_Table
            .Where(x => x.Id == sessionId && x.DisconnectedAt == null)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.LastSeen, now)
                .SetProperty(x => x.DegistirmeTarihi, now), cancellationToken);
    }

    public async Task StopAsync()
    {
        if (_cts is null)
            return;

        _cts.Cancel();
        if (_loop is not null)
            await _loop;
        _cts.Dispose();
        _cts = null;
        _loop = null;
    }

    public async ValueTask DisposeAsync() => await StopAsync();
}
