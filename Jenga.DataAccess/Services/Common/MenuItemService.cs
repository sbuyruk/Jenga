using Jenga.DataAccess.Data;
using Jenga.Models.Common;
using Jenga.Utility.Helpers;
using Jenga.Utility.Logging;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Services.Common
{
    public class MenuItemService : IMenuItemService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
        private readonly ILogService _logService;

        public MenuItemService(IDbContextFactory<ApplicationDbContext> dbFactory, ILogService logService)
        {
            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
            _logService = logService;
        }

        public async Task<List<Models.Common.MenuItem>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                return await db.MenuItem_Table.AsNoTracking().ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logService.LogError("MenuItemService.GetAllAsync", ex);
                return new();
            }
        }

        public async Task<Models.Common.MenuItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.MenuItem_Table.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public async Task<bool> AddAsync(Models.Common.MenuItem menuItem, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            await db.MenuItem_Table.AddAsync(menuItem, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> UpdateAsync(Models.Common.MenuItem menuItem, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            db.MenuItem_Table.Update(menuItem);
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> DeleteAsync(Models.Common.MenuItem menuItem, CancellationToken cancellationToken = default)
        {
            if (menuItem is null) return false;

            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            db.MenuItem_Table.Remove(menuItem);
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<List<MenuItem>> GetRecursiveMenuAsync()
        {
            await using var db = await _dbFactory.CreateDbContextAsync();
            var visible = await db.MenuItem_Table
                .AsNoTracking()
                .Where(m => m.IsVisible == true)
                .ToListAsync();

            visible.ForEach(m =>
                m.Url = string.IsNullOrWhiteSpace(m.Url) ? "#" : m.Url!
            );

            return MenuHelper.BuildTree(visible);
        }

        public async Task<List<MenuItem>> GetAuthorizedMenuAsync(int personelId)
        {
            await using var db = await _dbFactory.CreateDbContextAsync();

            var roleIds = await db.PersonelRol_Table
                .AsNoTracking()
                .Where(x => x.PersonelId == personelId)
                .Select(x => x.RoleId)
                .ToListAsync();

            if (roleIds.Count == 0)
            {
                return new List<MenuItem>();
            }

            var menuIds = await db.RolMenu_Table
                .AsNoTracking()
                .Where(x => roleIds.Contains(x.RoleId))
                .Select(x => x.MenuId)
                .ToListAsync();

            if (menuIds.Count == 0)
            {
                return new List<MenuItem>();
            }

            var allMenus = await db.MenuItem_Table
                .AsNoTracking()
                .Where(x => menuIds.Contains(x.Id) && x.IsVisible == true)
                .ToListAsync();

            allMenus.ForEach(m =>
                m.Url = string.IsNullOrWhiteSpace(m.Url) ? "#" : m.Url!
            );

            return MenuHelper.BuildTree(allMenus);
        }
    }
}
