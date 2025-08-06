using Jenga.Models.Common;

namespace Jenga.BlazorWeb.Services.Menu
{
    public interface IMenuService
    {
        Task<List<MenuItem>> GetRecursiveMenuAsync();
        Task<List<MenuItem>> GetAuthorizedMenuAsync(int personelId);
    }
}
