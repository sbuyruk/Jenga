using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repository.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jenga.DataAccess.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _db;
        public UnitOfWork(ApplicationDbContext db) 
        {
            _db = db;
            DepoTanim= new DepoTanimRepository(_db);
            KaynakTanim= new KaynakTanimRepository(_db);
            AniObjesiTanim= new AniObjesiTanimRepository(_db);
            DepoHareket= new DepoHareketRepository(_db);
            DepoStok= new DepoStokRepository(_db);
        }
        public IDepoTanimRepository DepoTanim { get; private set; }
        public IKaynakTanimRepository KaynakTanim { get; private set; }        
        public IAniObjesiTanimRepository AniObjesiTanim { get; private set; }        
        public IDepoHareketRepository DepoHareket { get; private set; }        
        public IDepoStokRepository DepoStok { get; private set; }        

        public void Save()
        {
            _db.SaveChanges();
        }
    }
}
