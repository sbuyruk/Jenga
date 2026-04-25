using Jenga.DataAccess.Repositories.IRepository;
using Jenga.Models.IKYS;

namespace Jenga.DataAccess.Services.IKYS;

public class YabanciDilService : IYabanciDilService
{
    private readonly IUnitOfWork _unitOfWork;

    public YabanciDilService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<YabanciDil>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _unitOfWork.YabanciDil.GetAllAsync(cancellationToken);

    public async Task<List<YabanciDil>> GetByPersonelIdAsync(int personelId, CancellationToken cancellationToken = default)
    {
        var all = await _unitOfWork.YabanciDil.GetAllAsync(cancellationToken);
        return all.Where(x => x.PersonelId == personelId).ToList();
    }

    public async Task<YabanciDil?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => await _unitOfWork.YabanciDil.GetByIdAsync(id, cancellationToken);

    public async Task<bool> AddAsync(YabanciDil entity, string? modifiedBy = null, CancellationToken cancellationToken = default)
    {
        entity.Olusturan = modifiedBy;
        entity.OlusturmaTarihi = DateTime.Now;
        await _unitOfWork.YabanciDil.AddAsync(entity, cancellationToken);
        await _unitOfWork.YabanciDil.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> UpdateAsync(YabanciDil entity, CancellationToken cancellationToken = default)
    {
        var existing = await GetByIdAsync(entity.Id, cancellationToken)
            ?? throw new Exception("Kayıt bulunamadı!");
        existing.PersonelId = entity.PersonelId;
        existing.Dil = entity.Dil;
        existing.SinavAdi = entity.SinavAdi;
        existing.SinavNotu = entity.SinavNotu;
        existing.SinavTarihi = entity.SinavTarihi;
        existing.Aciklama = entity.Aciklama;
        existing.Degistiren = entity.Degistiren;
        existing.DegistirmeTarihi = DateTime.Now;
        await _unitOfWork.YabanciDil.UpdateAsync(existing);
        await _unitOfWork.YabanciDil.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(YabanciDil entity, CancellationToken cancellationToken = default)
    {
        _unitOfWork.YabanciDil.Remove(entity);
        await _unitOfWork.YabanciDil.SaveChangesAsync(cancellationToken);
        return true;
    }
}
