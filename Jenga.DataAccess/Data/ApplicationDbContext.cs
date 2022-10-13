using Jenga.Models.MTS;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
