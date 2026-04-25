using Jenga.DataAccess.Repositories.IRepository;
using Jenga.Models.IKYS;

namespace Jenga.DataAccess.Services.IKYS;

public class KimlikService : IKimlikService
{
    private readonly IUnitOfWork _unitOfWork;

    public KimlikService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<Kimlik>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _unitOfWork.Kimlik.GetAllAsync(cancellationToken);

    public async Task<Kimlik?> GetByPersonelIdAsync(int personelId, CancellationToken cancellationToken = default)
    {
        var all = await _unitOfWork.Kimlik.GetAllAsync(cancellationToken);
        return all.FirstOrDefault(x => x.PersonelId == personelId);
    }

    public async Task<Kimlik?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => await _unitOfWork.Kimlik.GetByIdAsync(id, cancellationToken);

    public async Task<bool> AddAsync(Kimlik entity, string? modifiedBy = null, CancellationToken cancellationToken = default)
    {
        entity.Olusturan = modifiedBy;
        entity.OlusturmaTarihi = DateTime.Now;
        await _unitOfWork.Kimlik.AddAsync(entity, cancellationToken);
        await _unitOfWork.Kimlik.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> UpdateAsync(Kimlik entity, CancellationToken cancellationToken = default)
    {
        var existing = await GetByIdAsync(entity.Id, cancellationToken)
            ?? throw new Exception("Kayıt bulunamadı!");
        existing.PersonelId = entity.PersonelId;
        existing.TCKimlikNo = entity.TCKimlikNo;
        existing.BabaAdi = entity.BabaAdi;
        existing.AnneAdi = entity.AnneAdi;
        existing.DogumYeri = entity.DogumYeri;
        existing.DogumTar = entity.DogumTar;
        existing.MedeniHali = entity.MedeniHali;
        existing.EvlilikTar = entity.EvlilikTar;
        existing.Cinsiyet = entity.Cinsiyet;
        existing.EskiSoyadi = entity.EskiSoyadi;
        existing.KanGrubu = entity.KanGrubu;
        existing.DogumGunuKutlama = entity.DogumGunuKutlama;
        existing.EvlilikKutlama = entity.EvlilikKutlama;
        existing.Aciklama = entity.Aciklama;
        existing.Degistiren = entity.Degistiren;
        existing.DegistirmeTarihi = DateTime.Now;
        await _unitOfWork.Kimlik.UpdateAsync(existing);
        await _unitOfWork.Kimlik.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(Kimlik entity, CancellationToken cancellationToken = default)
    {
        _unitOfWork.Kimlik.Remove(entity);
        await _unitOfWork.Kimlik.SaveChangesAsync(cancellationToken);
        return true;
    }
}
