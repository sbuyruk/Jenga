using Jenga.BlazorUI.Services.Presence;
using Jenga.DataAccess.Data;
using Jenga.Models.Common;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Jenga.BlazorUI.Tests;

public sealed class PresenceTests
{
    [Fact]
    public async Task OnlineQueriesExcludeExpiredAndDisconnectedSessionsAndKeepLastUrl()
    {
        await using var database = await PresenceDatabase.CreateAsync();
        var clock = new TestClock();
        var now = clock.GetLocalNow().DateTime;
        await using (var db = database.CreateDbContext())
        {
            db.UserPresenceSession_Table.AddRange(
                Session(1, now), Session(2, now.AddSeconds(-91)),
                Session(3, now, now), Session(4, now.AddSeconds(-90)));
            db.UserNavigationEvent_Table.AddRange(
                new UserNavigationEvent { PersonelId = 1, PresenceSessionId = 1, Url = "/old", OccurredAt = now.AddMinutes(-1) },
                new UserNavigationEvent { PersonelId = 1, PresenceSessionId = 1, Url = "/home", OccurredAt = now });
            await db.SaveChangesAsync();
        }

        var query = new UserPresenceQueryService(database, clock);
        Assert.Equal(new[] { 1, 4 }, (await query.GetOpenSessionsAsync()).Select(x => x.Id).Order());
        var views = await query.GetOpenSessionViewsAsync();
        Assert.Equal(new[] { 1, 4 }, views.Select(x => x.Session.Id).Order());
        Assert.Equal("/home", views.Single(x => x.Session.Id == 1).LastUrl);

        clock.Advance(TimeSpan.FromSeconds(91));
        Assert.Empty(await query.GetOpenSessionsAsync());
        Assert.Empty(await query.GetOpenSessionViewsAsync());
    }

    [Fact]
    public async Task HeartbeatRefreshesOpenSessionWithoutReopeningClosedSessions()
    {
        await using var database = await PresenceDatabase.CreateAsync();
        var clock = new TestClock();
        var now = clock.GetLocalNow().DateTime;
        var old = now.AddMinutes(-5);
        await using (var db = database.CreateDbContext())
        {
            db.UserPresenceSession_Table.AddRange(Session(1, old), Session(2, old, old));
            await db.SaveChangesAsync();
        }

        await using var heartbeat = Heartbeat(database, clock, new());
        await heartbeat.TouchAsync(1);
        await heartbeat.TouchAsync(2);
        await heartbeat.TouchAsync(999);

        await using var read = database.CreateDbContext();
        var rows = await read.UserPresenceSession_Table.OrderBy(x => x.Id).ToListAsync();
        Assert.Equal(2, rows.Count);
        Assert.Equal(now, rows[0].LastSeen);
        Assert.Equal(now, rows[0].DegistirmeTarihi);
        Assert.Equal(old, rows[1].LastSeen);
        Assert.Equal(old, rows[1].DisconnectedAt);
    }

    [Fact]
    public async Task TimerUpdatesIdleSessionsAndStopsAndRestartsWithConnection()
    {
        await using var database = await PresenceDatabase.CreateAsync();
        var clock = new TestClock();
        var initial = clock.GetLocalNow().DateTime;
        await using (var db = database.CreateDbContext())
        {
            db.UserPresenceSession_Table.Add(Session(1, initial));
            await db.SaveChangesAsync();
        }

        var state = new PresenceHeartbeatState { SessionId = 1 };
        await using var heartbeat = Heartbeat(database, clock, state);
        await heartbeat.StartAsync();
        await heartbeat.StartAsync(); // Reconnect must not leave a second loop behind.
        Assert.Equal(1, clock.ActiveTimers);
        clock.Advance(PresenceHeartbeatService.Interval);
        await WaitForLastSeenAsync(database, clock.GetLocalNow().DateTime);

        await heartbeat.StopAsync();
        Assert.Equal(0, clock.ActiveTimers);
        clock.Advance(TimeSpan.FromMinutes(2));
        Assert.Empty(await new UserPresenceQueryService(database, clock).GetOpenSessionsAsync());

        await heartbeat.StartAsync();
        clock.Advance(PresenceHeartbeatService.Interval);
        await WaitForLastSeenAsync(database, clock.GetLocalNow().DateTime);
        Assert.Single(await new UserPresenceQueryService(database, clock).GetOpenSessionsAsync());
        await heartbeat.DisposeAsync();
        Assert.Equal(0, clock.ActiveTimers);
    }

    [Fact]
    public async Task NoSessionDoesNotStartTimer()
    {
        await using var database = await PresenceDatabase.CreateAsync();
        var clock = new TestClock();
        await using var heartbeat = Heartbeat(database, clock, new());
        await heartbeat.StartAsync();
        Assert.Equal(0, clock.ActiveTimers);
    }

    private static PresenceHeartbeatService Heartbeat(PresenceDatabase database, TestClock clock, PresenceHeartbeatState state)
        => new(database, state, clock, NullLogger<PresenceHeartbeatService>.Instance);

    private static UserPresenceSession Session(int id, DateTime seen, DateTime? disconnected = null)
        => new() { Id = id, PersonelId = id, CircuitId = $"circuit-{id}", ConnectedAt = seen, LastSeen = seen, DisconnectedAt = disconnected };

    private static async Task WaitForLastSeenAsync(PresenceDatabase database, DateTime expected)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        while (true)
        {
            await using var db = database.CreateDbContext();
            if (await db.UserPresenceSession_Table.Select(x => x.LastSeen).SingleAsync(timeout.Token) == expected)
                return;
            await Task.Delay(10, timeout.Token);
        }
    }

    private sealed class PresenceDatabase(string connectionString, SqliteConnection anchor)
        : IDbContextFactory<ApplicationDbContext>, IAsyncDisposable
    {
        public ApplicationDbContext CreateDbContext()
            => new PresenceContext(new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(connectionString).Options);

        public static async Task<PresenceDatabase> CreateAsync()
        {
            var connectionString = $"Data Source={Guid.NewGuid():N};Mode=Memory;Cache=Shared";
            var anchor = new SqliteConnection(connectionString);
            await anchor.OpenAsync();
            var factory = new PresenceDatabase(connectionString, anchor);
            await using var db = factory.CreateDbContext();
            await db.Database.EnsureCreatedAsync();
            return factory;
        }

        public ValueTask DisposeAsync() => anchor.DisposeAsync();
    }

    private sealed class PresenceContext(DbContextOptions<ApplicationDbContext> options) : ApplicationDbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            foreach (var type in modelBuilder.Model.GetEntityTypes().Select(x => x.ClrType)
                .Where(x => x != typeof(UserPresenceSession) && x != typeof(UserNavigationEvent)).ToArray())
                modelBuilder.Ignore(type);
            modelBuilder.Entity<UserPresenceSession>().ToTable("UserPresenceSession_Table");
            modelBuilder.Entity<UserNavigationEvent>().ToTable("UserNavigationEvent_Table");
        }
    }

    private sealed class TestClock : TimeProvider
    {
        private DateTimeOffset _now = new(2026, 11, 13, 12, 0, 0, TimeSpan.Zero);
        private readonly List<TestTimer> _timers = [];
        public override DateTimeOffset GetUtcNow() => _now;
        public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;
        public int ActiveTimers => _timers.Count(x => !x.Disposed);

        public override ITimer CreateTimer(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period)
        {
            var timer = new TestTimer(callback, state, _now + dueTime, period);
            _timers.Add(timer);
            return timer;
        }

        public void Advance(TimeSpan elapsed)
        {
            _now += elapsed;
            foreach (var timer in _timers.ToArray())
                timer.Fire(_now);
        }

        private sealed class TestTimer(TimerCallback callback, object? state, DateTimeOffset due, TimeSpan period) : ITimer
        {
            public bool Disposed { get; private set; }
            public bool Change(TimeSpan dueTime, TimeSpan newPeriod) => throw new NotSupportedException();
            public void Fire(DateTimeOffset now)
            {
                if (Disposed || now < due) return;
                due = now + period;
                callback(state);
            }
            public void Dispose() => Disposed = true;
            public ValueTask DisposeAsync() { Dispose(); return ValueTask.CompletedTask; }
        }
    }
}
