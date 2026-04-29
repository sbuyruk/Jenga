using Jenga.DataAccess.Repositories.IRepository;
using Jenga.Models.IKYS;

namespace Jenga.DataAccess.Services.IKYS;

public class DereceKademeDegisimService : IDereceKademeDegisimService
{
    private readonly IUnitOfWork _unitOfWork;

    public DereceKademeDegisimService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<DereceKademeDegisim>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _unitOfWork.DereceKademeDegisim.GetAllAsync(cancellationToken);

    public async Task<List<DereceKademeDegisim>> GetByPersonelIdAsync(int personelId, CancellationToken cancellationToken = default)
    {
        var all = await _unitOfWork.DereceKademeDegisim.GetAllAsync(cancellationToken);
        return all.Where(x => x.PersonelId == personelId).ToList();
    }

    public async Task<DereceKademeDegisim?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => await _unitOfWork.DereceKademeDegisim.GetByIdAsync(id, cancellationToken);

    public async Task<bool> AddAsync(DereceKademeDegisim entity, string? modifiedBy = null, CancellationToken cancellationToken = default)
    {
        entity.Olusturan = modifiedBy;
        entity.OlusturmaTarihi = DateTime.Now;
        await _unitOfWork.DereceKademeDegisim.AddAsync(entity, cancellationToken);
        await _unitOfWork.DereceKademeDegisim.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> UpdateAsync(DereceKademeDegisim entity, CancellationToken cancellationToken = default)
    {
        var existing = await GetByIdAsync(entity.Id, cancellationToken)
            ?? throw new Exception("Kayıt bulunamadı!");
        existing.PersonelId = entity.PersonelId;
        existing.Degisim = entity.Degisim;
        existing.DegisimTarihi = entity.DegisimTarihi;
        existing.Derece = entity.Derece;
        existing.Kademe = entity.Kademe;
        existing.Aciklama = entity.Aciklama;
        existing.Degistiren = entity.Degistiren;
        existing.DegistirmeTarihi = DateTime.Now;
        await _unitOfWork.DereceKademeDegisim.UpdateAsync(existing);
        await _unitOfWork.DereceKademeDegisim.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(DereceKademeDegisim entity, CancellationToken cancellationToken = default)
    {
        _unitOfWork.DereceKademeDegisim.Remove(entity);
        await _unitOfWork.DereceKademeDegisim.SaveChangesAsync(cancellationToken);
        return true;
    }
    public async Task<List<DereceKademeDegisim>> GetDereceYukseltmeAsync(CancellationToken cancellationToken = default)
    => await _unitOfWork.DereceKademeDegisim.GetDereceYukseltmeAsync(cancellationToken);

}
