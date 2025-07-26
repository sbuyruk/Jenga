using Jenga.Models.Ortak;

namespace Jenga.DataAccess.Repositories.IRepository.Ortak
{
    public interface IMenuTanimRepository : IRepository<MenuTanim>
    {

        public List<MenuTanim> GetMenusByIds(IEnumerable<int> menuTanimIds);
        public List<MenuTanimVM> GetSubMenuMenuListByParentId(int? parentId);

    }
}
