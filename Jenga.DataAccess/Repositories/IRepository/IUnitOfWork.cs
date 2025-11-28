using Jenga.DataAccess.Repositories.IRepository.Common;
using Jenga.DataAccess.Repositories.IRepository.IKYS;
using Jenga.DataAccess.Repositories.IRepository.Inventory;
using Jenga.DataAccess.Repositories.IRepository.Common;

namespace Jenga.DataAccess.Repositories.IRepository
{
    public interface IUnitOfWork
    {
        //Inventory
        IMaterialRepository Material { get; }
        IMaterialUnitRepository MaterialUnit { get; }
        IMaterialEntryRepository MaterialEntry { get; }
        IMaterialCategoryRepository MaterialCategory { get; }
        IMaterialBrandRepository MaterialBrand { get; }
        IMaterialModelRepository MaterialModel { get; }
        ILocationRepository Location { get; }
        IMaterialInventoryRepository MaterialInventory { get; }
        IMaterialMovementRepository MaterialMovement { get; }
        IMaterialAssignmentRepository MaterialAssignment { get; }
        IMaterialExitRepository MaterialExit { get; }
        IMaterialTransferRepository MaterialTransfer { get; }
        // Common
        IMenuItemRepository MenuItem { get; }
        IRoleRepository Role { get; }
        IRoleMenuRepository RoleMenu { get; }
        IPersonelRoleRepository PersonelRole { get; }
        IBolgeRepository Bolge { get; }
        IIlRepository Il { get; }
        IIlceRepository Ilce { get; }

        //IKYS
        IPersonelRepository Personel { get; }
        IPersonelLocationRepository PersonelLocation { get; }


    }
}
