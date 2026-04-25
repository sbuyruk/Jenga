using Jenga.DataAccess.Repositories.IRepository;
using Jenga.Models.IKYS;

namespace Jenga.DataAccess.Services.IKYS;

public class GorevTanimService : IGorevTanimService
{
    private readonly IUnitOfWork _unitOfWork;

    public GorevTanimService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<GorevTanim>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _unitOfWork.GorevTanim.GetAllAsync(cancellationToken);

    public async Task<List<GorevTanim>> GetByBirimIdAsync(int birimId, CancellationToken cancellationToken = default)
    {
        var all = await _unitOfWork.GorevTanim.GetAllAsync(cancellationToken);
        return all.Where(x => x.BirimId == birimId).ToList();
    }

    public async Task<GorevTanim?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => await _unitOfWork.GorevTanim.GetByIdAsync(id, cancellationToken);

    public async Task<bool> AddAsync(GorevTanim entity, string? modifiedBy = null, CancellationToken cancellationToken = default)
    {
        entity.Olusturan = modifiedBy;
        entity.OlusturmaTarihi = DateTime.Now;
        await _unitOfWork.GorevTanim.AddAsync(entity, cancellationToken);
        await _unitOfWork.GorevTanim.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> UpdateAsync(GorevTanim entity, CancellationToken cancellationToken = default)
    {
        var existing = await GetByIdAsync(entity.Id, cancellationToken)
            ?? throw new Exception("Kayıt bulunamadı!");
        existing.BirimId = entity.BirimId;
        existing.Adi = entity.Adi;
        existing.KisaAdi = entity.KisaAdi;
        existing.PersonelId = entity.PersonelId;
        existing.Vekil = entity.Vekil;
        existing.Aktif = entity.Aktif;
        existing.Aciklama = entity.Aciklama;
        existing.Degistiren = entity.Degistiren;
        existing.DegistirmeTarihi = DateTime.Now;
        await _unitOfWork.GorevTanim.UpdateAsync(existing);
        await _unitOfWork.GorevTanim.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(GorevTanim entity, CancellationToken cancellationToken = default)
    {
        _unitOfWork.GorevTanim.Remove(entity);
        await _unitOfWork.GorevTanim.SaveChangesAsync(cancellationToken);
        return true;
    }
}
