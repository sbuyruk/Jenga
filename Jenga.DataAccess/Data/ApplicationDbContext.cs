using Jenga.Models.Common;
using Jenga.Models.FTK;
using Jenga.Models.IKYS;
using Jenga.Models.Inventory;
using Jenga.Models.NBYS;
using Jenga.Models.TBYS;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }
        //Common
        public DbSet<Bolge> Bolge_Table { get; set; }
        public DbSet<Il> Il_Table { get; set; }
        public DbSet<Ilce> Ilce_Table { get; set; }
        public DbSet<MenuItem> MenuItem_Table { get; set; }
        public DbSet<Role> Rol_Table { get; set; }
        public DbSet<PersonelRole> PersonelRol_Table { get; set; }
        public DbSet<RoleMenu> RolMenu_Table { get; set; }
        //Presence
        public DbSet<UserPresenceSession> UserPresenceSession_Table { get; set; }
        public DbSet<UserNavigationEvent> UserNavigationEvent_Table { get; set; }

        //Inventory
        public DbSet<Material> Material_Table { get; set; }
        public DbSet<MaterialEntry> MaterialEntry_Table { get; set; }
        public DbSet<MaterialUnit> MaterialUnit_Table { get; set; }
        public DbSet<MaterialCategory> MaterialCategory_Table { get; set; }
        public DbSet<MaterialBrand> MaterialBrand_Table { get; set; }
        public DbSet<MaterialModel> MaterialModel_Table { get; set; }
        public DbSet<Location> Location_Table { get; set; }
        public DbSet<MaterialInventory> MaterialInventory_Table { get; set; }
        public DbSet<MaterialMovement> MaterialMovement_Table { get; set; }
        public DbSet<MaterialAssignment> MaterialAssignment_Table { get; set; }
        public DbSet<MaterialExit> MaterialExit_Table { get; set; }
        public DbSet<MaterialTransfer> MaterialTransfer_Table { get; set; }
        public DbSet<MaterialAsset> MaterialAsset_Table { get; set; }
        public DbSet<MaterialAssetLog> MaterialAssetLog_Table { get; set; }

        //IKYS
        public DbSet<Personel> Personel_Table { get; set; }
        public DbSet<IsBilgileri> IsBilgileri_Table { get; set; }
        public DbSet<PersonelLocation> PersonelLocation_Table { get; set; }

        // TBYS
        public DbSet<Tasinmaz> Tasinmaz_Table { get; set; }
        public DbSet<TasinmazBagisci> TasinmazBagisci_Table { get; set; }
        public DbSet<Bagis> Bagis_Table { get; set; }
        public DbSet<Kiraci> Kiraci_Table { get; set; }
        public DbSet<KiraSozlesme> KiraSozlesme_Table { get; set; }
        public DbSet<SozlesmeTasinmaz> SozlesmeTasinmaz_Table { get; set; }
        public DbSet<OdemePlani> OdemePlani_Table { get; set; }
        public DbSet<Odeme> Odeme_Table { get; set; }
        //NBYS
        public DbSet<NakitBagisci> NakitBagisci_Table { get; set; }
        public DbSet<NakitBagisHareket> NakitBagisHareket_Table { get; set; }
        public DbSet<Armagan> Armagan_Table { get; set; }
        public DbSet<BankaTanim> BankaTanim_Table { get; set; }
        public DbSet<ArmaganTanim> ArmaganTanim_Table { get; set; }
        public DbSet<DuzenliNakitBagisci> DuzenliNakitBagisci_Table { get; set; }
        public DbSet<YasalFaiz> YasalFaiz_Table { get; set; }
        //FTK
        public DbSet<Ftk> FTK_Table { get; set; }
        public DbSet<FtkIslem> FTKIslem_Table { get; set; }
        public DbSet<FtkKisi> FTKKisi_Table { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserPresenceSession>()
                .HasIndex(x => x.CircuitId)
                .IsUnique();

            modelBuilder.Entity<UserPresenceSession>()
                .HasIndex(x => new { x.PersonelId, x.DisconnectedAt });
        }
    }
}
