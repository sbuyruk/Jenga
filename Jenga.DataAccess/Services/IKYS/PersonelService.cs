using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jenga.DataAccess.Repositories.IRepository;
using Jenga.Models;
using Jenga.Models.IKYS;

namespace Jenga.DataAccess.Services.IKYS
{
    public class PersonelService : IPersonelService
    {
        private readonly IUnitOfWork _unitOfWork;

        public PersonelService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<Personel>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _unitOfWork.Personel.GetAllAsync(cancellationToken);

        public async Task<Personel?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => await _unitOfWork.Personel.GetByIdAsync(id, cancellationToken);

        public async Task<bool> AddAsync(Personel personel, string? modifiedBy = null, CancellationToken cancellationToken = default)
        {
            await _unitOfWork.Personel.AddAsync(personel, cancellationToken);
            await _unitOfWork.Personel.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> UpdateAsync(Personel personel, CancellationToken cancellationToken = default)
        {
            var existing = await GetByIdAsync(personel.Id, cancellationToken);
            if (existing == null) throw new Exception("Kayıt bulunamadı!");

            // Update fields (simple replace)
            existing.Adi = personel.Adi;
            existing.Soyadi = personel.Soyadi;
            existing.KullaniciAdi = personel.KullaniciAdi;
            existing.Asker_sivil = personel.Asker_sivil;
            existing.Aciklama = personel.Aciklama;
            existing.SicilNo = personel.SicilNo;
            existing.Tahsili = personel.Tahsili;

            await _unitOfWork.Personel.UpdateAsync(existing);
            await _unitOfWork.Personel.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> DeleteAsync(Personel personel, CancellationToken cancellationToken = default)
        {
            _unitOfWork.Personel.Remove(personel);
            await _unitOfWork.Personel.SaveChangesAsync(cancellationToken);
            return true;
        }

        public Task<bool> AnyAsync(Expression<Func<Personel, bool>> predicate)
        {
            return _unitOfWork.Personel.AnyAsync(predicate);
        }

        public async Task<bool> UpdatePersonelAndSaveAsync(Personel personel, string? currentUserName, CancellationToken cancellationToken = default)
        {
            return await UpdateAsync(personel, cancellationToken);
        }

        public async Task<bool> DeletePersonelAndSaveAsync(Personel personel, string? currentUserName, CancellationToken cancellationToken = default)
        {
            return await DeleteAsync(personel, cancellationToken);
        }

        public async Task<List<Personel>> GetCalisanPersonelAsync(CancellationToken cancellationToken = default)
        {
            // include IsBilgileri to evaluate working status
            var list = (await _unitOfWork.Personel.GetAllAsync("IsBilgileri")).ToList();

            var calisan = list.Where(p => p.IsBilgileri != null
                && p.IsBilgileri.CalismaDurumu.Equals("1"))
                .ToList();

            return calisan;
        }
    }
}