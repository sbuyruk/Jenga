using Jenga.Models.Common;
using Jenga.Models.DYS;
using Jenga.Models.IKYS;
using Jenga.Models.Inventory;
using Jenga.Models.MTS;
using Jenga.Models.NBYS;
using Jenga.Models.Ortak;
using Jenga.Models.TBYS;
using Jenga.Models.TYS;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }
        //Common
        public DbSet<MenuItem> MenuItem_Table { get; set; }
        public DbSet<Rol> Rol_Table { get; set; }
        public DbSet<PersonelRol> PersonelRol_Table{ get; set; }
        public DbSet<RolMenu> RolMenu_Table{ get; set; }
        public DbSet<PersonelMenu> PersonelMenuleri { get; set; }
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

        //IKYS
        public DbSet<Personel> Personel_Table { get; set; }
        public DbSet<PersonelMenu> PersonelMenu_Table { get; set; }
        public DbSet<IsBilgileri> IsBilgileri_Table { get; set; }
        public DbSet<GorevTanim> GorevTanim_Table { get; set; }
        public DbSet<BirimTanim> BirimTanim_Table { get; set; }
        public DbSet<UnvanTanim> UnvanTanim_Table { get; set; }
        public DbSet<Kimlik> Kimlik_Table { get; set; }
        public DbSet<IletisimBilgileri> IletisimBilgileri_Table { get; set; }
        public DbSet<ResmiTatil> ResmiTatil_Table { get; set; }
        //MTS
        public DbSet<DepoTanim> DepoTanim_Table { get; set; }
        public DbSet<KaynakTanim> KaynakTanim_Table { get; set; }
        public DbSet<AniObjesiTanim> AniObjesiTanim_Table { get; set; }
        public DbSet<DepoHareket> DepoHareket_Table { get; set; }
        public DbSet<DepoStok> DepoStok_Table { get; set; }
        public DbSet<ModulTanim> ModulTanim_Table { get; set; }
        public DbSet<MenuTanim> MenuTanim_Table { get; set; }
        public DbSet<DagitimYeriTanim> DagitimYeriTanim_Table { get; set; }
        public DbSet<GonderiPaketi> GonderiPaketi_Table { get; set; }
        public DbSet<Kisi> Kisi_Table { get; set; }
        public DbSet<AniObjesiDagitim> AniObjesiDagitim_Table { get; set; }
        public DbSet<MTSKurumTanim> MTSKurumTanim_Table { get; set; }
        public DbSet<MTSGorevTanim> MTSGorevTanim_Table { get; set; }
        public DbSet<MTSUnvanTanim> MTSUnvanTanim_Table { get; set; }
        public DbSet<MTSKurumGorev> MTSKurumGorev_Table { get; set; }
        public DbSet<FaaliyetKatilim> FaaliyetKatilim_Table { get; set; }
        public DbSet<Faaliyet> Faaliyet_Table { get; set; }
        public DbSet<FaaliyetAmaci> FaaliyetAmaci_Table { get; set; }
        public DbSet<AramaGorusme> AramaGorusme_Table { get; set; }
        //TBYS
        public DbSet<TasinmazBagisci> TasinmazBagisci_Table { get; set; }
        //NBYS
        public DbSet<NakitBagisci> NakitBagisci_Table { get; set; }
        public DbSet<NakitBagisHareket> NakitBagisHareket_Table { get; set; }
        //TYS
        public DbSet<Toplanti> Toplanti_Table { get; set; }
        public DbSet<ToplantiKatilim> ToplantiKatilim_Table { get; set; }
        //ortak
        public DbSet<Bolge> Bolge_Table { get; set; }
        public DbSet<Il> Il_Table { get; set; }
        public DbSet<Ilce> Ilce_Table { get; set; }
        //DYS
        public DbSet<EnvanterTanim> EnvanterTanim_Table { get; set; }
        public DbSet<MalzemeGrubu> MalzemeGrubu_Table { get; set; }
        public DbSet<MalzemeCinsi> MalzemeCinsi_Table { get; set; }
        public DbSet<MarkaTanim> MarkaTanim_Table { get; set; }
        public DbSet<ModelTanim> ModelTanim_Table { get; set; }
        public DbSet<Ozellik> Ozellik_Table { get; set; }
        public DbSet<MalzemeOzellik> MalzemeOzellik_Table { get; set; }
        public DbSet<Malzeme> Malzeme_Table { get; set; }
        public DbSet<MalzemeYeriTanim> MalzemeYeriTanim_Table { get; set; }
        public DbSet<MalzemeDagilim> MalzemeDagilim_Table { get; set; }
        public DbSet<MalzemeHareket> MalzemeHareket_Table { get; set; }
        public DbSet<Zimmet> Zimmet_Table { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            //composite primary key 
            //modelBuilder.Entity<MalzemeDagilim>()
            //    .HasKey(od => new { od.MalzemeId, od.MalzemeYeriTanimId });
            modelBuilder.Entity<MalzemeDagilim>()
                .Property(m => m.Id)
                .ValueGeneratedOnAdd(); // Ensures EF Core knows that Id is auto-generated
            modelBuilder.Entity<MalzemeHareket>()
                .Property(m => m.Id)
                .ValueGeneratedOnAdd(); // Ensures EF Core knows that Id is auto-generated

            //Latest
            base.OnModelCreating(modelBuilder);

            // Common 
            //modelBuilder.Entity<MenuItem>()
            //    .ToTable("MenuItem");

        }
    }
}
