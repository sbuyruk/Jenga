using Jenga.DataAccess.Data;
using Jenga.Models.Common;
using Jenga.Models.Helpers;
using Jenga.Utility.Logging;
using Jenga.Utility.Results;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Services.Common
{
    public class MenuItemService : IMenuItemService
    {
        private const string Source = nameof(MenuItemService);
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
        private readonly ILogService _logService;

        public MenuItemService(IDbContextFactory<ApplicationDbContext> dbFactory, ILogService logService)
        {
            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
            _logService = logService ?? throw new ArgumentNullException(nameof(logService));
        }

        public async Task<Result<List<MenuItem>>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var list = await db.MenuItem_Table.AsNoTracking().ToListAsync(cancellationToken);
                return Result.Success(list);
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"{Source}.GetAllAsync");
                return Result.Failure<List<MenuItem>>(Error.Unexpected("Menü kayitlari getirilemedi.", ex, "Menu.GetAll.Failed"));
            }
        }

        public async Task<Result<MenuItem>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var entity = await db.MenuItem_Table.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
                if (entity is null)
                    return Result.Failure<MenuItem>(Error.NotFound($"Menü bulunamadi (Id={id}).", "Menu.NotFound"));
                return Result.Success(entity);
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"{Source}.GetByIdAsync");
                return Result.Failure<MenuItem>(Error.Unexpected("Menü getirilemedi.", ex, "Menu.GetById.Failed"));
            }
        }

        public async Task<Result> AddAsync(MenuItem menuItem, CancellationToken cancellationToken = default)
        {
            if (menuItem is null)
                return Result.Failure(Error.Validation("Menü bos olamaz.", "Menu.Null"));
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                await db.MenuItem_Table.AddAsync(menuItem, cancellationToken);
                await db.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"{Source}.AddAsync");
                return Result.Failure(Error.Unexpected("Menü eklenemedi.", ex, "Menu.Add.Failed"));
            }
        }

        public async Task<Result> UpdateAsync(MenuItem menuItem, CancellationToken cancellationToken = default)
        {
            if (menuItem is null)
                return Result.Failure(Error.Validation("Menü bos olamaz.", "Menu.Null"));
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                db.MenuItem_Table.Update(menuItem);
                await db.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"{Source}.UpdateAsync");
                return Result.Failure(Error.Unexpected("Menü güncellenemedi.", ex, "Menu.Update.Failed"));
            }
        }

        public async Task<Result> DeleteAsync(MenuItem menuItem, CancellationToken cancellationToken = default)
        {
            if (menuItem is null)
                return Result.Failure(Error.Validation("Menü bos olamaz.", "Menu.Null"));
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                db.MenuItem_Table.Remove(menuItem);
                await db.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"{Source}.DeleteAsync");
                return Result.Failure(Error.Unexpected("Menü silinemedi.", ex, "Menu.Delete.Failed"));
            }
        }

        public async Task<Result<List<MenuItem>>> GetRecursiveMenuAsync()
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync();
                var visible = await db.MenuItem_Table
                    .AsNoTracking()
                    .Where(m => m.IsVisible == true)
                    .ToListAsync();

                visible.ForEach(m =>
                    m.Url = string.IsNullOrWhiteSpace(m.Url) ? "#" : m.Url!
                );

                return Result.Success(MenuHelper.BuildTree(visible, msg => _logService.LogWarning(msg)));
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"{Source}.GetRecursiveMenuAsync");
                return Result.Failure<List<MenuItem>>(Error.Unexpected("Menü agaci olusturulamadi.", ex, "Menu.Recursive.Failed"));
            }
        }

        public async Task<Result<List<MenuItem>>> GetAuthorizedMenuAsync(int personelId)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync();

                var roleIds = await db.PersonelRol_Table
                    .AsNoTracking()
                    .Where(x => x.PersonelId == personelId)
                    .Select(x => x.RoleId)
                    .ToListAsync();

                if (roleIds.Count == 0)
                {
                    return Result.Success(new List<MenuItem>());
                }

                var menuIds = await db.RolMenu_Table
                    .AsNoTracking()
                    .Where(x => roleIds.Contains(x.RoleId))
                    .Select(x => x.MenuId)
                    .ToListAsync();

                if (menuIds.Count == 0)
                {
                    return Result.Success(new List<MenuItem>());
                }

                var allMenus = await db.MenuItem_Table
                    .AsNoTracking()
                    .Where(x => menuIds.Contains(x.Id) && x.IsVisible == true)
                    .ToListAsync();

                allMenus.ForEach(m =>
                    m.Url = string.IsNullOrWhiteSpace(m.Url) ? "#" : m.Url!
                );

                return Result.Success(MenuHelper.BuildTree(allMenus, msg => _logService.LogWarning(msg)));
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"{Source}.GetAuthorizedMenuAsync");
                return Result.Failure<List<MenuItem>>(Error.Unexpected("Yetkili menü olusturulamadi.", ex, "Menu.Authorized.Failed"));
            }
        }
    }
}
