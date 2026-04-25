using Jenga.DataAccess.Repositories.IRepository;
using Jenga.Models.IKYS;

namespace Jenga.DataAccess.Services.IKYS;

public class IzinHareketService : IIzinHareketService
{
    private readonly IUnitOfWork _unitOfWork;

    public IzinHareketService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<IzinHareket>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _unitOfWork.IzinHareket.GetAllAsync(cancellationToken);

    public async Task<List<IzinHareket>> GetByPersonelIdAsync(int personelId, CancellationToken cancellationToken = default)
    {
        var all = await _unitOfWork.IzinHareket.GetAllAsync(cancellationToken);
        return all.Where(x => x.PersonelId == personelId).ToList();
    }

    public async Task<IzinHareket?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => await _unitOfWork.IzinHareket.GetByIdAsync(id, cancellationToken);

    public async Task<bool> AddAsync(IzinHareket entity, string? modifiedBy = null, CancellationToken cancellationToken = default)
    {
        entity.Olusturan = modifiedBy;
        entity.OlusturmaTarihi = DateTime.Now;
        entity.Mahsup ??= false;
        await _unitOfWork.IzinHareket.AddAsync(entity, cancellationToken);
        await _unitOfWork.IzinHareket.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> UpdateAsync(IzinHareket entity, CancellationToken cancellationToken = default)
    {
        var existing = await GetByIdAsync(entity.Id, cancellationToken)
            ?? throw new Exception("Kayıt bulunamadı!");
        existing.PersonelId = entity.PersonelId;
        existing.IzinTalepId = entity.IzinTalepId;
        existing.IzinDonemId = entity.IzinDonemId;
        existing.IzinTipi = entity.IzinTipi;
        existing.BaslangicTarihi = entity.BaslangicTarihi;
        existing.BitisTarihi = entity.BitisTarihi;
        existing.Sure = entity.Sure;
        existing.Birim = entity.Birim;
        existing.Adres = entity.Adres;
        existing.VekilImza = entity.VekilImza;
        existing.AmirImza = entity.AmirImza;
        existing.OnayImza = entity.OnayImza;
        existing.Mahsup = entity.Mahsup;
        existing.OncekiIzinStr = entity.OncekiIzinStr;
        existing.KullanilanIzinStr = entity.KullanilanIzinStr;
        existing.KalanIzinStr = entity.KalanIzinStr;
        existing.Aciklama = entity.Aciklama;
        existing.Degistiren = entity.Degistiren;
        existing.DegistirmeTarihi = DateTime.Now;
        await _unitOfWork.IzinHareket.UpdateAsync(existing);
        await _unitOfWork.IzinHareket.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(IzinHareket entity, CancellationToken cancellationToken = default)
    {
        _unitOfWork.IzinHareket.Remove(entity);
        await _unitOfWork.IzinHareket.SaveChangesAsync(cancellationToken);
        return true;
    }
}
