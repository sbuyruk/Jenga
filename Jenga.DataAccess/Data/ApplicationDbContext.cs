using Jenga.Models.IKYS;
using Jenga.Models.MTS;
using Jenga.Models.Ortak;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }
        public DbSet<DepoTanim> DepoTanim_Table { get; set; }
        public DbSet<KaynakTanim> KaynakTanim_Table { get; set; }
        public DbSet<AniObjesiTanim> AniObjesiTanim_Table { get; set; }
        public DbSet<DepoHareket> DepoHareket_Table { get; set; }
        public DbSet<DepoStok> DepoStok_Table { get; set; }
        public DbSet<ModulTanim> ModulTanim_Table { get; set; }
        public DbSet<MenuTanim> MenuTanim_Table { get; set; }
        public DbSet<Personel> Personel_Table { get; set; }
        public DbSet<PersonelMenu> PersonelMenu_Table { get; set; }
        public DbSet<IsBilgileri> IsBilgileri_Table { get; set; }
        public DbSet<GorevTanim> GorevTanim_Table { get; set; }
        public DbSet<BirimTanim> BirimTanim_Table { get; set; }
        public DbSet<UnvanTanim> UnvanTanim_Table { get; set; }
    }
}
