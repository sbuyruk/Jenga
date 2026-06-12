using Jenga.DataAccess.Services.Common;
using Jenga.Models.Common;

namespace Jenga.BlazorUI.Services.Common
{
    public class MenuStateService
    {
        public Guid InstanceId { get; } = Guid.NewGuid();
        public List<MenuItem>? MenuItems { get; private set; }

        private readonly MenuItemService _menuService;
        private readonly SemaphoreSlim _gate = new(1, 1);

        // UI state
        private bool _isOpen = true;
        private bool _isCollapsed;

        public bool IsOpen
        {
            get => _isOpen;
            private set
            {
                if (_isOpen == value) return;
                _isOpen = value;
                NotifyChange();
            }
        }

        public bool IsCollapsed
        {
            get => _isCollapsed;
            private set
            {
                if (_isCollapsed == value) return;
                _isCollapsed = value;
                NotifyChange();
            }
        }

        public event Action? OnChange;

        private void NotifyChange() => OnChange?.Invoke();

        public MenuStateService(MenuItemService menuService)
        {
            _menuService = menuService;
            Console.WriteLine($"MenuStateService created: {InstanceId}");
        }

        public async Task EnsureLoadedAsync(int userId = 127)
        {
            if (MenuItems is { Count: > 0 }) return;
            await LoadAsync(userId);
        }

        public async Task ReloadAsync(int userId = 127)
        {
            MenuItems = null;
            await LoadAsync(userId);
            NotifyChange();
        }

        private async Task LoadAsync(int userId)
        {
            await _gate.WaitAsync();
            try
            {
                if (MenuItems is { Count: > 0 }) return; // double-check after awaiting
                var result = await _menuService.GetAuthorizedMenuAsync(userId);
                MenuItems = result.IsSuccess ? result.Value : new List<MenuItem>();
            }
            finally
            {
                _gate.Release();
            }
        }

        // helpers for components to call
        public void ToggleOpen() => IsOpen = !IsOpen;
        public void SetOpen(bool open) => IsOpen = open;
        public void ToggleCollapsed() => IsCollapsed = !IsCollapsed;
        public void SetCollapsed(bool collapsed) => IsCollapsed = collapsed;
    }
}