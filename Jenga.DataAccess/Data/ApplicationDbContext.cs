using Jenga.Models.IKYS;
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
        

    }
}
