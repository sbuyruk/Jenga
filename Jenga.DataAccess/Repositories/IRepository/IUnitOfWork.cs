using Jenga.DataAccess.Repositories.IRepository.Common;
using Jenga.DataAccess.Repositories.IRepository.FTK;
using Jenga.DataAccess.Repositories.IRepository.IKYS;
using Jenga.DataAccess.Repositories.IRepository.Inventory;
using Jenga.DataAccess.Repositories.IRepository.NBYS;
using Jenga.DataAccess.Repositories.IRepository.TBYS;

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
        IMaterialAssetRepository MaterialAsset { get; }
        IMaterialAssetLogRepository MaterialAssetLog { get; }
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

        //TBYS
        ITasinmazRepository Tasinmaz { get; }
        ITasinmazBagisciRepository TasinmazBagisci { get; }
        IBagisRepository Bagis { get; }
        IKiraciRepository Kiraci { get; }
        IKiraSozlesmeRepository KiraSozlesme { get; }
        ISozlesmeTasinmazRepository SozlesmeTasinmaz { get; }
        IOdemePlaniRepository OdemePlani { get; }
        IOdemeRepository Odeme { get; }
        IYasalFaizRepository YasalFaiz { get; }
        //NBYS
        INakitBagisciRepository NakitBagisci { get; }
        INakitBagisHareketRepository NakitBagisHareket { get; }
        IArmaganRepository Armagan { get; }
        IBankaTanimRepository BankaTanim { get; }
        IArmaganTanimRepository ArmaganTanim { get; }
        IDuzenliNakitBagisciRepository DuzenliNakitBagisci { get; }
        //FTK
        IFtkRepository Ftk { get; }
        IFtkIslemRepository FtkIslem { get; }
        IFtkKisiRepository FtkKisi { get; }
    }
}
