using Jenga.DataAccess.Repositories.IRepository;
using Jenga.Models.IKYS;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Services.IKYS;

public class AileService : IAileService
{
    private readonly IUnitOfWork _unitOfWork;

    public AileService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<Aile>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _unitOfWork.Aile.GetAllAsync(cancellationToken);

    public async Task<List<Aile>> GetByPersonelIdAsync(int personelId, CancellationToken cancellationToken = default)
    {
        var all = await _unitOfWork.Aile.GetAllAsync(cancellationToken);
        return all.Where(x => x.PersonelId == personelId).ToList();
    }

    public async Task<Aile?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => await _unitOfWork.Aile.GetByIdAsync(id, cancellationToken);

    public async Task<bool> AddAsync(Aile entity, string? modifiedBy = null, CancellationToken cancellationToken = default)
    {
        entity.Olusturan = modifiedBy;
        entity.OlusturmaTarihi = DateTime.Now;
        await _unitOfWork.Aile.AddAsync(entity, cancellationToken);
        await _unitOfWork.Aile.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> UpdateAsync(Aile entity, CancellationToken cancellationToken = default)
    {
        var existing = await GetByIdAsync(entity.Id, cancellationToken)
            ?? throw new Exception("Kayıt bulunamadı!");
        existing.PersonelId = entity.PersonelId;
        existing.Adi = entity.Adi;
        existing.Soyadi = entity.Soyadi;
        existing.TcKimlikNo = entity.TcKimlikNo;
        existing.YakinlikDerecesi = entity.YakinlikDerecesi;
        existing.DogumTar = entity.DogumTar;
        existing.Tahsil = entity.Tahsil;
        existing.Okul = entity.Okul;
        existing.Telefon = entity.Telefon;
        existing.Meslek = entity.Meslek;
        existing.Aciklama = entity.Aciklama;
        existing.Degistiren = entity.Degistiren;
        existing.DegistirmeTarihi = DateTime.Now;
        await _unitOfWork.Aile.UpdateAsync(existing);
        await _unitOfWork.Aile.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(Aile entity, CancellationToken cancellationToken = default)
    {
        _unitOfWork.Aile.Remove(entity);
        await _unitOfWork.Aile.SaveChangesAsync(cancellationToken);
        return true;
    }
}
