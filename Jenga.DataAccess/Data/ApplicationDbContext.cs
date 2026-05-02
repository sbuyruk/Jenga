using Jenga.Models.Common;
using Jenga.Models.Enums;
using Jenga.Models.FTK;
using Jenga.Models.IKYS;
using Jenga.Models.Inventory;
using Jenga.Models.NBYS;
using Jenga.Models.Sistem;
using Jenga.Models.TBYS;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Jenga.DataAccess.Data
{
    public class ApplicationDbContext : DbContext
    {
        private string? _currentUser;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        /// <summary>
        /// Audit alanlarına yazılacak kullanıcı adını ayarlar.
        /// Servis katmanı AddAsync/UpdateAsync çağrısından önce bu metodu çağırmalıdır.
        /// </summary>
        public void SetCurrentUser(string? userName) => _currentUser = userName;

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var now = DateTime.Now;

            foreach (var entry in ChangeTracker.Entries<IAuditableEntity>())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.OlusturmaTarihi ??= now;
                    entry.Entity.Olusturan ??= _currentUser;
                }

                if (entry.State is EntityState.Added or EntityState.Modified)
                {
                    entry.Entity.DegistirmeTarihi = now;
                    entry.Entity.Degistiren = _currentUser ?? entry.Entity.Degistiren;
                }
            }

            return base.SaveChangesAsync(cancellationToken);
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
        public DbSet<MaterialExit> MaterialExit_Table { get; set; }
        public DbSet<MaterialTransfer> MaterialTransfer_Table { get; set; }
        public DbSet<MaterialAsset> MaterialAsset_Table { get; set; }
        public DbSet<MaterialAssetLog> MaterialAssetLog_Table { get; set; }

        //IKYS
        public DbSet<Personel> Personel_Table { get; set; }
        public DbSet<Kimlik> Kimlik_Table { get; set; }
        public DbSet<IsBilgileri> IsBilgileri_Table { get; set; }
        public DbSet<IletisimBilgileri> IletisimBilgileri_Table { get; set; }
        public DbSet<PersonelLocation> PersonelLocation_Table { get; set; }
        public DbSet<Aile> Aile_Table { get; set; }
        public DbSet<DereceKademeDegisim> DereceKademeDegisim_Table { get; set; }
        public DbSet<EgitimSeviyesi> EgitimSeviyesi_Table { get; set; }
        public DbSet<GorevOnay> GorevOnay_Table { get; set; }
        public DbSet<BirimTanim> BirimTanim_Table { get; set; }
        public DbSet<GorevTanim> GorevTanim_Table { get; set; }
        public DbSet<IzinTanim> IzinTanim_Table { get; set; }
        public DbSet<IzinDonem> IzinDonem_Table { get; set; }
        public DbSet<IzinTalep> IzinTalep_Table { get; set; }
        public DbSet<IzinHareket> IzinHareket_Table { get; set; }
        public DbSet<YabanciDil> YabanciDil_Table { get; set; }
        public DbSet<TahsilTanim> TahsilTanim_Table { get; set; }
        // IKYS bölümüne diğer DbSet'lerin yanına eklenecek:
        public DbSet<UnvanTanim> UnvanTanim_Table { get; set; }

        // TBYS
        public DbSet<Tasinmaz> Tasinmaz_Table { get; set; }
        public DbSet<TasinmazBagisci> TasinmazBagisci_Table { get; set; }
        public DbSet<Bagis> Bagis_Table { get; set; }
        public DbSet<Kiraci> Kiraci_Table { get; set; }
        public DbSet<KiraSozlesme> KiraSozlesme_Table { get; set; }
        public DbSet<SozlesmeTasinmaz> SozlesmeTasinmaz_Table { get; set; }
        public DbSet<OdemePlani> OdemePlani_Table { get; set; }
        public DbSet<Odeme> Odeme_Table { get; set; }
        public DbSet<BagisciTalepleri> BagisciTalepleri_Table { get; set; }
        public DbSet<BagisciYakinlari> BagisciYakinlari_Table { get; set; }
        public DbSet<TasinmazTaahhut> TasinmazTaahhut_Table { get; set; }
        public DbSet<Vasiyetci> Vasiyetci_Table { get; set; }
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

            // Material adı için filtered unique index.
            // SQL Server varsayılan collation (CI_AS) sayesinde "Çay" ve "ÇAY" aynı kabul edilir;
            // ek olarak NULL/boş adlar dışlanır. Race-condition'ı veritabanı seviyesinde kapatır.
            // NOT: Bu projede EF Core migration kullanılmıyor; index'i veritabanına manuel
            // uygulamak için altta verilen SQL snippet'ini çalıştırın.
            modelBuilder.Entity<Material>()
                .HasIndex(m => m.MaterialName)
                .IsUnique()
                .HasDatabaseName("UX_Material_MaterialName")
                .HasFilter("[MaterialName] IS NOT NULL");

            // Asker_sivil sütunu veritabanında "0"/"1" string olarak tutulduğundan
            // EF Core'a özel string <-> AskerSivil enum dönüşümü yapmasını söylüyoruz.
            var askerSivilConverter = new ValueConverter<AskerSivil?, string?>(
                v => v.HasValue ? ((int)v.Value).ToString() : null,
                s => s == null ? null : (AskerSivil?)int.Parse(s));
            modelBuilder.Entity<Personel>()
                .Property(p => p.AskerSivil)
                .HasConversion(askerSivilConverter);

            // CalismaDurumu sütunu veritabanında "0"/"1" string olarak tutulduğundan
            // EF Core'a özel string <-> CalismaDurumu enum dönüşümü yapmasını söylüyoruz.
            var calismaDurumuConverter = new ValueConverter<CalismaDurumu?, string?>(
                v => v.HasValue ? ((int)v.Value).ToString() : null,
                s => s == null ? null : (CalismaDurumu?)int.Parse(s));
            modelBuilder.Entity<IsBilgileri>()
                .Property(i => i.CalismaDurumu)
                .HasConversion(calismaDurumuConverter);
        }
    }
}
