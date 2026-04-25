using Jenga.DataAccess.Repositories.IRepository;
using Jenga.Models.IKYS;

namespace Jenga.DataAccess.Services.IKYS;

public class IzinTalepService : IIzinTalepService
{
    private readonly IUnitOfWork _unitOfWork;

    public IzinTalepService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<IzinTalep>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _unitOfWork.IzinTalep.GetAllAsync(cancellationToken);

    public async Task<List<IzinTalep>> GetByPersonelIdAsync(int personelId, CancellationToken cancellationToken = default)
    {
        var all = await _unitOfWork.IzinTalep.GetAllAsync(cancellationToken);
        return all.Where(x => x.PersonelId == personelId).ToList();
    }

    public async Task<IzinTalep?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => await _unitOfWork.IzinTalep.GetByIdAsync(id, cancellationToken);

    public async Task<bool> AddAsync(IzinTalep entity, string? modifiedBy = null, CancellationToken cancellationToken = default)
    {
        entity.Olusturan = modifiedBy;
        entity.OlusturmaTarihi = DateTime.Now;
        entity.Aktif ??= true;
        entity.OnayDurumu ??= 0;
        await _unitOfWork.IzinTalep.AddAsync(entity, cancellationToken);
        await _unitOfWork.IzinTalep.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> UpdateAsync(IzinTalep entity, CancellationToken cancellationToken = default)
    {
        var existing = await GetByIdAsync(entity.Id, cancellationToken)
            ?? throw new Exception("Kayıt bulunamadı!");
        existing.PersonelId = entity.PersonelId;
        existing.IzinTipi = entity.IzinTipi;
        existing.BaslangicTarihi = entity.BaslangicTarihi;
        existing.BitisTarihi = entity.BitisTarihi;
        existing.Sure = entity.Sure;
        existing.Birim = entity.Birim;
        existing.VekilImza = entity.VekilImza;
        existing.AmirImza = entity.AmirImza;
        existing.OnayImza = entity.OnayImza;
        existing.Adres = entity.Adres;
        existing.Aktif = entity.Aktif;
        existing.IzinDonemId = entity.IzinDonemId;
        existing.OnayDurumu = entity.OnayDurumu;
        existing.EPostaGonder = entity.EPostaGonder;
        existing.Aciklama = entity.Aciklama;
        existing.Degistiren = entity.Degistiren;
        existing.DegistirmeTarihi = DateTime.Now;
        await _unitOfWork.IzinTalep.UpdateAsync(existing);
        await _unitOfWork.IzinTalep.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(IzinTalep entity, CancellationToken cancellationToken = default)
    {
        _unitOfWork.IzinTalep.Remove(entity);
        await _unitOfWork.IzinTalep.SaveChangesAsync(cancellationToken);
        return true;
    }
}
