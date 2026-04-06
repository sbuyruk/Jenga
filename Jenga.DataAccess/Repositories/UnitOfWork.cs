using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repositories.Common;
using Jenga.DataAccess.Repositories.IKYS;
using Jenga.DataAccess.Repositories.Inventory;
using Jenga.DataAccess.Repositories.IRepository;
using Jenga.DataAccess.Repositories.IRepository.Common;
using Jenga.DataAccess.Repositories.IRepository.IKYS;
using Jenga.DataAccess.Repositories.IRepository.Inventory;
using Jenga.DataAccess.Repositories.IRepository.NBYS;
using Jenga.DataAccess.Repositories.IRepository.TBYS;
using Jenga.DataAccess.Repositories.NBYS;
using Jenga.DataAccess.Repositories.TBYS;
using Jenga.Models.TBYS;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public UnitOfWork(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;

            MenuItem = new MenuItemRepository(_contextFactory);
            Role = new RoleRepository(_contextFactory);
            RoleMenu = new RoleMenuRepository(_contextFactory);
            PersonelRole = new PersonelRoleRepository(_contextFactory);

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
            MaterialAsset = new MaterialAssetRepository(_contextFactory);
            MaterialAssetLog = new MaterialAssetLogRepository(_contextFactory);

            // Ortak
            Bolge = new BolgeRepository(_contextFactory);
            Il = new IlRepository(_contextFactory);
            Ilce = new IlceRepository(_contextFactory);

            // IKYS
            Personel = new PersonelRepository(_contextFactory);
            PersonelLocation = new PersonelLocationRepository(_contextFactory);

            // TBYS
            Tasinmaz = new TasinmazRepository(_contextFactory);
            TasinmazBagisci = new TasinmazBagisciRepository(_contextFactory);
            Bagis = new BagisRepository(_contextFactory);
            Kiraci = new KiraciRepository(_contextFactory);
            KiraSozlesme = new KiraSozlesmeRepository(_contextFactory);
            SozlesmeTasinmaz = new SozlesmeTasinmazRepository(_contextFactory);
            OdemePlani = new OdemePlaniRepository(_contextFactory);
            Odeme = new OdemeRepository(_contextFactory);
            YasalFaiz = new YasalFaizRepository(_contextFactory);

            // NBYS
            NakitBagisci = new NakitBagisciRepository(_contextFactory);
            NakitBagisHareket = new NakitBagisHareketRepository(_contextFactory);
            Armagan = new ArmaganRepository(_contextFactory);
            BankaTanim = new BankaTanimRepository(_contextFactory);
        }

        // Common
        public IMenuItemRepository MenuItem { get; private set; }
        public IRoleRepository Role { get; private set; }
        public IRoleMenuRepository RoleMenu { get; private set; }
        public IPersonelRoleRepository PersonelRole { get; private set; }
        public IBolgeRepository Bolge { get; private set; }
        public IIlRepository Il { get; private set; }
        public IIlceRepository Ilce { get; private set; }

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
        public IMaterialAssetRepository MaterialAsset { get; private set; }
        public IMaterialAssetLogRepository MaterialAssetLog { get; private set; }

        // IKYS
        public IPersonelRepository Personel { get; private set; }
        public IPersonelLocationRepository PersonelLocation { get; private set; }

        // TBYS
        public ITasinmazRepository Tasinmaz { get; private set; }
        public ITasinmazBagisciRepository TasinmazBagisci { get; private set; }
        public IBagisRepository Bagis { get; private set; }
        public IKiraciRepository Kiraci { get; private set; }
        public IKiraSozlesmeRepository KiraSozlesme { get; private set; }
        public ISozlesmeTasinmazRepository SozlesmeTasinmaz { get; private set; }
        public IOdemePlaniRepository OdemePlani { get; private set; }
        public IOdemeRepository Odeme { get; private set; }
        public IYasalFaizRepository YasalFaiz { get; private set; }

        // NBYS
        public INakitBagisciRepository NakitBagisci { get; private set; }
        public INakitBagisHareketRepository NakitBagisHareket { get; private set; }
        public IArmaganRepository Armagan { get; private set; }
        public IBankaTanimRepository BankaTanim { get; private set; }
    }
}