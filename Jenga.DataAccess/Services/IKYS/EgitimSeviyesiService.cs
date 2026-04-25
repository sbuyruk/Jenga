using Jenga.DataAccess.Repositories.IRepository;
using Jenga.Models.IKYS;

namespace Jenga.DataAccess.Services.IKYS;

public class EgitimSeviyesiService : IEgitimSeviyesiService
{
    private readonly IUnitOfWork _unitOfWork;

    public EgitimSeviyesiService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<EgitimSeviyesi>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _unitOfWork.EgitimSeviyesi.GetAllAsync(cancellationToken);

    public async Task<EgitimSeviyesi?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => await _unitOfWork.EgitimSeviyesi.GetByIdAsync(id, cancellationToken);

    public async Task<bool> AddAsync(EgitimSeviyesi entity, string? modifiedBy = null, CancellationToken cancellationToken = default)
    {
        entity.Olusturan = modifiedBy;
        entity.OlusturmaTarihi = DateTime.Now;
        await _unitOfWork.EgitimSeviyesi.AddAsync(entity, cancellationToken);
        await _unitOfWork.EgitimSeviyesi.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> UpdateAsync(EgitimSeviyesi entity, CancellationToken cancellationToken = default)
    {
        var existing = await GetByIdAsync(entity.Id, cancellationToken)
            ?? throw new Exception("Kayıt bulunamadı!");
        existing.Adi = entity.Adi;
        existing.KisaAdi = entity.KisaAdi;
        existing.Aciklama = entity.Aciklama;
        existing.Degistiren = entity.Degistiren;
        existing.DegistirmeTarihi = DateTime.Now;
        await _unitOfWork.EgitimSeviyesi.UpdateAsync(existing);
        await _unitOfWork.EgitimSeviyesi.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(EgitimSeviyesi entity, CancellationToken cancellationToken = default)
    {
        _unitOfWork.EgitimSeviyesi.Remove(entity);
        await _unitOfWork.EgitimSeviyesi.SaveChangesAsync(cancellationToken);
        return true;
    }
}
