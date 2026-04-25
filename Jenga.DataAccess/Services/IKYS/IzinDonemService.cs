using Jenga.DataAccess.Repositories.IRepository;
using Jenga.Models.IKYS;

namespace Jenga.DataAccess.Services.IKYS;

public class IzinDonemService : IIzinDonemService
{
    private readonly IUnitOfWork _unitOfWork;

    public IzinDonemService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<IzinDonem>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _unitOfWork.IzinDonem.GetAllAsync(cancellationToken);

    public async Task<List<IzinDonem>> GetByPersonelIdAsync(int personelId, CancellationToken cancellationToken = default)
    {
        var all = await _unitOfWork.IzinDonem.GetAllAsync(cancellationToken);
        return all.Where(x => x.PersonelId == personelId).ToList();
    }

    public async Task<IzinDonem?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => await _unitOfWork.IzinDonem.GetByIdAsync(id, cancellationToken);

    public async Task<bool> AddAsync(IzinDonem entity, string? modifiedBy = null, CancellationToken cancellationToken = default)
    {
        entity.Olusturan = modifiedBy;
        entity.OlusturmaTarihi = DateTime.Now;
        await _unitOfWork.IzinDonem.AddAsync(entity, cancellationToken);
        await _unitOfWork.IzinDonem.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> UpdateAsync(IzinDonem entity, CancellationToken cancellationToken = default)
    {
        var existing = await GetByIdAsync(entity.Id, cancellationToken)
            ?? throw new Exception("Kayıt bulunamadı!");
        existing.PersonelId = entity.PersonelId;
        existing.BaslangicTarihi = entity.BaslangicTarihi;
        existing.BitisTarihi = entity.BitisTarihi;
        existing.Adi = entity.Adi;
        existing.IzinTipi = entity.IzinTipi;
        existing.IzinHakki = entity.IzinHakki;
        existing.KullanilanIzin = entity.KullanilanIzin;
        existing.KalanIzin = entity.KalanIzin;
        existing.Birim = entity.Birim;
        existing.Aciklama = entity.Aciklama;
        existing.Degistiren = entity.Degistiren;
        existing.DegistirmeTarihi = DateTime.Now;
        await _unitOfWork.IzinDonem.UpdateAsync(existing);
        await _unitOfWork.IzinDonem.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(IzinDonem entity, CancellationToken cancellationToken = default)
    {
        _unitOfWork.IzinDonem.Remove(entity);
        await _unitOfWork.IzinDonem.SaveChangesAsync(cancellationToken);
        return true;
    }
}
