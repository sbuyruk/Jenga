using Jenga.DataAccess.Repositories.IRepository;
using Jenga.Models.IKYS;

namespace Jenga.DataAccess.Services.IKYS;

public class IletisimBilgileriService : IIletisimBilgileriService
{
    private readonly IUnitOfWork _unitOfWork;

    public IletisimBilgileriService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<IletisimBilgileri>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _unitOfWork.IletisimBilgileri.GetAllAsync(cancellationToken);

    public async Task<IletisimBilgileri?> GetByPersonelIdAsync(int personelId, CancellationToken cancellationToken = default)
    {
        var all = await _unitOfWork.IletisimBilgileri.GetAllAsync(cancellationToken);
        return all.FirstOrDefault(x => x.PersonelId == personelId);
    }

    public async Task<IletisimBilgileri?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => await _unitOfWork.IletisimBilgileri.GetByIdAsync(id, cancellationToken);

    public async Task<bool> AddAsync(IletisimBilgileri entity, string? modifiedBy = null, CancellationToken cancellationToken = default)
    {
        entity.Olusturan = modifiedBy;
        entity.OlusturmaTarihi = DateTime.Now;
        await _unitOfWork.IletisimBilgileri.AddAsync(entity, cancellationToken);
        await _unitOfWork.IletisimBilgileri.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> UpdateAsync(IletisimBilgileri entity, CancellationToken cancellationToken = default)
    {
        var existing = await GetByIdAsync(entity.Id, cancellationToken)
            ?? throw new Exception("Kayıt bulunamadı!");
        existing.PersonelId = entity.PersonelId;
        existing.Adres = entity.Adres;
        existing.Semt = entity.Semt;
        existing.Ili = entity.Ili;
        existing.Ilcesi = entity.Ilcesi;
        existing.PostaKodu = entity.PostaKodu;
        existing.DahiliTelefonu = entity.DahiliTelefonu;
        existing.EvTelefonu = entity.EvTelefonu;
        existing.CepTelefonu = entity.CepTelefonu;
        existing.CepTelefonu2 = entity.CepTelefonu2;
        existing.IntranetEPosta = entity.IntranetEPosta;
        existing.InternetEPosta = entity.InternetEPosta;
        existing.OzelEPosta = entity.OzelEPosta;
        existing.Aciklama = entity.Aciklama;
        existing.Degistiren = entity.Degistiren;
        existing.DegistirmeTarihi = DateTime.Now;
        await _unitOfWork.IletisimBilgileri.UpdateAsync(existing);
        await _unitOfWork.IletisimBilgileri.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(IletisimBilgileri entity, CancellationToken cancellationToken = default)
    {
        _unitOfWork.IletisimBilgileri.Remove(entity);
        await _unitOfWork.IletisimBilgileri.SaveChangesAsync(cancellationToken);
        return true;
    }
}
