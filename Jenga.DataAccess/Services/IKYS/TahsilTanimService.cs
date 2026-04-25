using Jenga.DataAccess.Repositories.IRepository;
using Jenga.Models.IKYS;

namespace Jenga.DataAccess.Services.IKYS;

public class TahsilTanimService : ITahsilTanimService
{
    private readonly IUnitOfWork _unitOfWork;

    public TahsilTanimService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<TahsilTanim>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _unitOfWork.TahsilTanim.GetAllAsync(cancellationToken);

    public async Task<TahsilTanim?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => await _unitOfWork.TahsilTanim.GetByIdAsync(id, cancellationToken);

    public async Task<bool> AddAsync(TahsilTanim entity, string? modifiedBy = null, CancellationToken cancellationToken = default)
    {
        entity.Olusturan = modifiedBy;
        entity.OlusturmaTarihi = DateTime.Now;
        await _unitOfWork.TahsilTanim.AddAsync(entity, cancellationToken);
        await _unitOfWork.TahsilTanim.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> UpdateAsync(TahsilTanim entity, CancellationToken cancellationToken = default)
    {
        var existing = await GetByIdAsync(entity.Id, cancellationToken)
            ?? throw new Exception("Kayıt bulunamadı!");
        existing.TahsilDurumu = entity.TahsilDurumu;
        existing.Degistiren = entity.Degistiren;
        existing.DegistirmeTarihi = DateTime.Now;
        await _unitOfWork.TahsilTanim.UpdateAsync(existing);
        await _unitOfWork.TahsilTanim.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(TahsilTanim entity, CancellationToken cancellationToken = default)
    {
        _unitOfWork.TahsilTanim.Remove(entity);
        await _unitOfWork.TahsilTanim.SaveChangesAsync(cancellationToken);
        return true;
    }
}
