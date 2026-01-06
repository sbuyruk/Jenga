using Jenga.DataAccess.Services.Common;
using Jenga.Models.Common;

namespace Jenga.BlazorUI.Services.Common
{
    public class MenuStateService
    {
        public Guid InstanceId { get; } = Guid.NewGuid();
        public List<MenuItem>? MenuItems { get; private set; }

        private readonly IMenuItemService _menuService;
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

        public MenuStateService(IMenuItemService menuService)
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

        // helpers for components to call
        public void ToggleOpen() => IsOpen = !IsOpen;
        public void SetOpen(bool open) => IsOpen = open;
        public void ToggleCollapsed() => IsCollapsed = !IsCollapsed;
        public void SetCollapsed(bool collapsed) => IsCollapsed = collapsed;
    }
}