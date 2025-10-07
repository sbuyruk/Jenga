using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repositories.IRepository.Inventory;
using Jenga.Models.Inventory;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Repositories.Inventory
{
    public class LocationRepository : Repository<Location>, ILocationRepository
    {
        private readonly ApplicationDbContext _db;
        public LocationRepository(ApplicationDbContext db) : base(db) { _db = db; }



    }
}