using Jenga.DataAccess.Repositories.IRepository;
using Jenga.Models.IKYS;

namespace Jenga.DataAccess.Services.IKYS;

public class BirimTanimService : IBirimTanimService
{
    private readonly IUnitOfWork _unitOfWork;

    public BirimTanimService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<BirimTanim>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _unitOfWork.BirimTanim.GetAllAsync(cancellationToken);

    public async Task<BirimTanim?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => await _unitOfWork.BirimTanim.GetByIdAsync(id, cancellationToken);

    public async Task<bool> AddAsync(BirimTanim entity, string? modifiedBy = null, CancellationToken cancellationToken = default)
    {
        entity.Olusturan = modifiedBy;
        entity.OlusturmaTarihi = DateTime.Now;
        await _unitOfWork.BirimTanim.AddAsync(entity, cancellationToken);
        await _unitOfWork.BirimTanim.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> UpdateAsync(BirimTanim entity, CancellationToken cancellationToken = default)
    {
        var existing = await GetByIdAsync(entity.Id, cancellationToken)
            ?? throw new Exception("Kayıt bulunamadı!");
        existing.Adi = entity.Adi;
        existing.KisaAdi = entity.KisaAdi;
        existing.ParentId = entity.ParentId;
        existing.AmirId = entity.AmirId;
        existing.Sira = entity.Sira;
        existing.Aktif = entity.Aktif;
        existing.BolgeId = entity.BolgeId;
        existing.Aciklama = entity.Aciklama;
        existing.Degistiren = entity.Degistiren;
        existing.DegistirmeTarihi = DateTime.Now;
        await _unitOfWork.BirimTanim.UpdateAsync(existing);
        await _unitOfWork.BirimTanim.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(BirimTanim entity, CancellationToken cancellationToken = default)
    {
        _unitOfWork.BirimTanim.Remove(entity);
        await _unitOfWork.BirimTanim.SaveChangesAsync(cancellationToken);
        return true;
    }
}
