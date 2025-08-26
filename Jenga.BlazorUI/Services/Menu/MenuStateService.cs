using Jenga.Models.Common;

namespace Jenga.BlazorUI.Services.Menu
{
    public class MenuStateService
    {
        public Guid InstanceId { get; } = Guid.NewGuid();
        public List<MenuItem>? MenuItems { get; private set; }

        private readonly IMenuService _menuService;
        private readonly SemaphoreSlim _gate = new(1, 1);

        public MenuStateService(IMenuService menuService)
        {
            _menuService = menuService;
            Console.WriteLine($"MenuStateService created: {InstanceId}");
        }

        public async Task EnsureLoadedAsync(int userId = 127)
        {
            if (MenuItems is { Count: > 0 }) return;

            await _gate.WaitAsync();
            try
            {
                if (MenuItems is { Count: > 0 }) return; // double-check after awaiting
                MenuItems = await _menuService.GetAuthorizedMenuAsync(userId);
            }
            finally
            {
                _gate.Release();
            }
        }
    }

}
