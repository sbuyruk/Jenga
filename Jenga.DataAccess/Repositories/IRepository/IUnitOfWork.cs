using Jenga.DataAccess.Repositories.IRepository.IKYS;
using Jenga.DataAccess.Repositories.IRepository.Inventory;
using Jenga.DataAccess.Repositories.IRepository.Menu;
using Jenga.DataAccess.Repositories.IRepository.Ortak;

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
        // Menu
        IMenuItemRepository MenuItem { get; }
        IRolRepository Rol { get; }
        IRolMenuRepository RolMenu { get; }
        IPersonelRolRepository PersonelRol { get; }
        
        //Ortak
        IBolgeRepository Bolge { get; }
        IIlRepository Il { get; }
        IIlceRepository Ilce { get; }

        //IKYS
        IPersonelRepository Personel { get; }


    }
}
