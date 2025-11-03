using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repositories.IKYS;
using Jenga.DataAccess.Repositories.Inventory;
using Jenga.DataAccess.Repositories.IRepository;
using Jenga.DataAccess.Repositories.IRepository.IKYS;
using Jenga.DataAccess.Repositories.IRepository.Inventory;
using Jenga.DataAccess.Repositories.IRepository.Menu;
using Jenga.DataAccess.Repositories.IRepository.Ortak;
using Jenga.DataAccess.Repositories.Menu;
using Jenga.DataAccess.Repositories.Ortak;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Repositories
{
    // UnitOfWork now receives IDbContextFactory<ApplicationDbContext> and
    // constructs repositories that themselves accept the factory.
    // This avoids creating and holding long-lived ApplicationDbContext instances
    // inside the UnitOfWork, preventing concurrent usage issues in Blazor Server.
    public class UnitOfWork : IUnitOfWork
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public UnitOfWork(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;

            // Create repositories passing the factory. Each repository will create
            // short-lived DbContext instances per operation.
            MenuItem = new MenuItemRepository(_contextFactory);
            Rol = new RolRepository(_contextFactory);
            RolMenu = new RolMenuRepository(_contextFactory);
            PersonelRol = new PersonelRolRepository(_contextFactory);

            // Inventory
            Material = new MaterialRepository(_contextFactory);
            MaterialEntry = new MaterialEntryRepository(_contextFactory);
            MaterialUnit = new MaterialUnitRepository(_contextFactory);
            MaterialCategory = new MaterialCategoryRepository(_contextFactory);
            MaterialBrand = new MaterialBrandRepository(_contextFactory);
            MaterialModel = new MaterialModelRepository(_contextFactory);
            Location = new LocationRepository(_contextFactory);
            MaterialInventory = new MaterialInventoryRepository(_contextFactory);
            MaterialMovement = new MaterialMovementRepository(_contextFactory);
            MaterialAssignment = new MaterialAssignmentRepository(_contextFactory);
            MaterialExit = new MaterialExitRepository(_contextFactory);
            MaterialTransfer = new MaterialTransferRepository(_contextFactory);

            // Ortak
            Bolge = new BolgeRepository(_contextFactory);
            Il = new IlRepository(_contextFactory);
            Ilce = new IlceRepository(_contextFactory);


            // IKYS
            Personel = new PersonelRepository(_contextFactory);


        }

        // Common
        public IMenuItemRepository MenuItem { get; private set; }
        public IRolRepository Rol { get; private set; }
        public IRolMenuRepository RolMenu { get; private set; }
        public IPersonelRolRepository PersonelRol { get; private set; }

        // Inventory
        public IMaterialRepository Material { get; private set; }
        public IMaterialEntryRepository MaterialEntry { get; private set; }
        public IMaterialUnitRepository MaterialUnit { get; private set; }
        public IMaterialCategoryRepository MaterialCategory { get; private set; }
        public IMaterialBrandRepository MaterialBrand { get; private set; }
        public IMaterialModelRepository MaterialModel { get; private set; }
        public ILocationRepository Location { get; private set; }
        public IMaterialInventoryRepository MaterialInventory { get; private set; }
        public IMaterialMovementRepository MaterialMovement { get; private set; }
        public IMaterialAssignmentRepository MaterialAssignment { get; private set; }
        public IMaterialExitRepository MaterialExit { get; private set; }
        public IMaterialTransferRepository MaterialTransfer { get; private set; }



        // Ortak
        public IBolgeRepository Bolge { get; private set; }
        public IIlRepository Il { get; private set; }
        public IIlceRepository Ilce { get; private set; }


        // IKYS
        public IPersonelRepository Personel { get; private set; }

    }
}