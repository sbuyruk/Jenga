using Jenga.Models.Common;
using Jenga.Models.IKYS;
using Jenga.Models.Inventory;
using Jenga.Models.Ortak;
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
        public DbSet<PersonelRol> PersonelRol_Table { get; set; }
        public DbSet<RolMenu> RolMenu_Table { get; set; }
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
        public DbSet<MaterialTransfer> MaterialTransfer_Table { get; set; }

        //IKYS
        public DbSet<Personel> Personel_Table { get; set; }

        //ortak
        public DbSet<Bolge> Bolge_Table { get; set; }
        public DbSet<Il> Il_Table { get; set; }
        public DbSet<Ilce> Ilce_Table { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

        }
    }
}
