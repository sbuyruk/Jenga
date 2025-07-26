using Jenga.Models.Common;

namespace Jenga.BlazorUI.Services.Menu
{
    public interface IMenuService
    {
        Task<List<MenuItem>> GetRecursiveMenuAsync();
    }
}
