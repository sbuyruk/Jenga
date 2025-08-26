using Jenga.Models.Common;

namespace Jenga.BlazorUI.Services.Menu
{
    public interface IMenuService
    {
        Task<List<MenuItem>> GetAllAsync();
        Task<List<MenuItem>> GetRecursiveMenuAsync();
        Task<List<MenuItem>> GetAuthorizedMenuAsync(int personelId);
    }
}
