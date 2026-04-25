using Jenga.DataAccess.Repositories.IRepository;
using Jenga.Models.IKYS;

namespace Jenga.DataAccess.Services.IKYS;

public class IzinTanimService : IIzinTanimService
{
    private readonly IUnitOfWork _unitOfWork;

    public IzinTanimService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<IzinTanim>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _unitOfWork.IzinTanim.GetAllAsync(cancellationToken);

    public async Task<IzinTanim?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => await _unitOfWork.IzinTanim.GetByIdAsync(id, cancellationToken);

    public async Task<bool> AddAsync(IzinTanim entity, string? modifiedBy = null, CancellationToken cancellationToken = default)
    {
        entity.Olusturan = modifiedBy;
        entity.OlusturmaTarihi = DateTime.Now;
        await _unitOfWork.IzinTanim.AddAsync(entity, cancellationToken);
        await _unitOfWork.IzinTanim.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> UpdateAsync(IzinTanim entity, CancellationToken cancellationToken = default)
    {
        var existing = await GetByIdAsync(entity.Id, cancellationToken)
            ?? throw new Exception("Kayıt bulunamadı!");
        existing.Adi = entity.Adi;
        existing.Aciklama = entity.Aciklama;
        existing.Degistiren = entity.Degistiren;
        existing.DegistirmeTarihi = DateTime.Now;
        await _unitOfWork.IzinTanim.UpdateAsync(existing);
        await _unitOfWork.IzinTanim.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(IzinTanim entity, CancellationToken cancellationToken = default)
    {
        _unitOfWork.IzinTanim.Remove(entity);
        await _unitOfWork.IzinTanim.SaveChangesAsync(cancellationToken);
        return true;
    }
}
