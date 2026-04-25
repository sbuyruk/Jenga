using Jenga.DataAccess.Repositories.IRepository;
using Jenga.Models.IKYS;

namespace Jenga.DataAccess.Services.IKYS;

public class GorevOnayService : IGorevOnayService
{
    private readonly IUnitOfWork _unitOfWork;

    public GorevOnayService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<GorevOnay>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _unitOfWork.GorevOnay.GetAllAsync(cancellationToken);

    public async Task<List<GorevOnay>> GetByPersonelIdAsync(int personelId, CancellationToken cancellationToken = default)
    {
        var all = await _unitOfWork.GorevOnay.GetAllAsync(cancellationToken);
        return all.Where(x => x.PersonelId == personelId).ToList();
    }

    public async Task<GorevOnay?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => await _unitOfWork.GorevOnay.GetByIdAsync(id, cancellationToken);

    public async Task<bool> AddAsync(GorevOnay entity, string? modifiedBy = null, CancellationToken cancellationToken = default)
    {
        entity.Olusturan = modifiedBy;
        entity.OlusturmaTarihi = DateTime.Now;
        await _unitOfWork.GorevOnay.AddAsync(entity, cancellationToken);
        await _unitOfWork.GorevOnay.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> UpdateAsync(GorevOnay entity, CancellationToken cancellationToken = default)
    {
        var existing = await GetByIdAsync(entity.Id, cancellationToken)
            ?? throw new Exception("Kayıt bulunamadı!");
        existing.PersonelId = entity.PersonelId;
        existing.GorevinSebebi = entity.GorevinSebebi;
        existing.GorevinYeri = entity.GorevinYeri;
        existing.BaslangicTarihi = entity.BaslangicTarihi;
        existing.BitisTarihi = entity.BitisTarihi;
        existing.Sure = entity.Sure;
        existing.Avans = entity.Avans;
        existing.Yevmiye = entity.Yevmiye;
        existing.ParaBirimi = entity.ParaBirimi;
        existing.AracTahsisi = entity.AracTahsisi;
        existing.AracPlakasi = entity.AracPlakasi;
        existing.PerSubeImza = entity.PerSubeImza;
        existing.PerSubeVekil = entity.PerSubeVekil;
        existing.OnayImza = entity.OnayImza;
        existing.OnayMakam = entity.OnayMakam;
        existing.OnayMakamVekil = entity.OnayMakamVekil;
        existing.GMImza = entity.GMImza;
        existing.GMVekil = entity.GMVekil;
        existing.UlasimAraci = entity.UlasimAraci;
        existing.Secildi = entity.Secildi;
        existing.GunlukYevmiye = entity.GunlukYevmiye;
        existing.Odendi = entity.Odendi;
        existing.Aciklama = entity.Aciklama;
        existing.Degistiren = entity.Degistiren;
        existing.DegistirmeTarihi = DateTime.Now;
        await _unitOfWork.GorevOnay.UpdateAsync(existing);
        await _unitOfWork.GorevOnay.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(GorevOnay entity, CancellationToken cancellationToken = default)
    {
        _unitOfWork.GorevOnay.Remove(entity);
        await _unitOfWork.GorevOnay.SaveChangesAsync(cancellationToken);
        return true;
    }
}
