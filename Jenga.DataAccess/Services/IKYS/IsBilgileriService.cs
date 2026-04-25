using Jenga.DataAccess.Repositories.IRepository;
using Jenga.Models.IKYS;

namespace Jenga.DataAccess.Services.IKYS;

public class IsBilgileriService : IIsBilgileriService
{
    private readonly IUnitOfWork _unitOfWork;

    public IsBilgileriService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<IsBilgileri>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _unitOfWork.IsBilgileri.GetAllAsync(cancellationToken);

    public async Task<IsBilgileri?> GetByPersonelIdAsync(int personelId, CancellationToken cancellationToken = default)
    {
        var all = await _unitOfWork.IsBilgileri.GetAllAsync(cancellationToken);
        return all.FirstOrDefault(x => x.PersonelId == personelId);
    }

    public async Task<IsBilgileri?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => await _unitOfWork.IsBilgileri.GetByIdAsync(id, cancellationToken);

    public async Task<bool> AddAsync(IsBilgileri entity, string? modifiedBy = null, CancellationToken cancellationToken = default)
    {
        entity.Olusturan = modifiedBy;
        entity.OlusturmaTarihi = DateTime.Now;
        await _unitOfWork.IsBilgileri.AddAsync(entity, cancellationToken);
        await _unitOfWork.IsBilgileri.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> UpdateAsync(IsBilgileri entity, CancellationToken cancellationToken = default)
    {
        var existing = await GetByIdAsync(entity.Id, cancellationToken)
            ?? throw new Exception("Kayıt bulunamadı!");
        existing.PersonelId = entity.PersonelId;
        existing.UnvanId = entity.UnvanId;
        existing.GorevId = entity.GorevId;
        existing.BirimId = entity.BirimId;
        existing.BaslamaTar = entity.BaslamaTar;
        existing.CalismaDurumu = entity.CalismaDurumu;
        existing.AyrilmaTar = entity.AyrilmaTar;
        existing.AyrilmaSebebi = entity.AyrilmaSebebi;
        existing.SGKSicilNo = entity.SGKSicilNo;
        existing.SGKBasTar = entity.SGKBasTar;
        existing.VakifOncesiPrimGunSayisi = entity.VakifOncesiPrimGunSayisi;
        existing.EmeklilikTarihi = entity.EmeklilikTarihi;
        existing.IzinDonemiBasTar = entity.IzinDonemiBasTar;
        existing.Aciklama = entity.Aciklama;
        existing.Degistiren = entity.Degistiren;
        existing.DegistirmeTarihi = DateTime.Now;
        await _unitOfWork.IsBilgileri.UpdateAsync(existing);
        await _unitOfWork.IsBilgileri.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(IsBilgileri entity, CancellationToken cancellationToken = default)
    {
        _unitOfWork.IsBilgileri.Remove(entity);
        await _unitOfWork.IsBilgileri.SaveChangesAsync(cancellationToken);
        return true;
    }
}
