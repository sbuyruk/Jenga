using Jenga.DataAccess.Repositories.IRepository;
using Jenga.Models.TBYS;
using Jenga.Utility.Logging;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Services.TBYS;

public class YasalFaizService : IYasalFaizService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogService _logService;

    public YasalFaizService(IUnitOfWork unitOfWork, ILogService logService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logService = logService;
    }

    public async Task<List<YasalFaiz>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _unitOfWork.YasalFaiz.GetAllAsync(cancellationToken);

    public async Task<YasalFaiz?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => await _unitOfWork.YasalFaiz.GetByIdAsync(id, cancellationToken);

    public async Task<bool> AddAsync(YasalFaiz yasalFaiz, CancellationToken cancellationToken = default)
    {
        if (yasalFaiz == null) throw new ArgumentNullException(nameof(yasalFaiz));

        try
        {
            await _unitOfWork.YasalFaiz.AddAsync(yasalFaiz, cancellationToken);
            await _unitOfWork.YasalFaiz.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logService?.LogError("Yasal faiz eklenirken hata oluştu.", ex);
            throw;
        }
    }

    public async Task<bool> UpdateAsync(YasalFaiz yasalFaiz, CancellationToken cancellationToken = default)
    {
        if (yasalFaiz == null) throw new ArgumentNullException(nameof(yasalFaiz));

        try
        {
            await _unitOfWork.YasalFaiz.UpdateAsync(yasalFaiz, null, cancellationToken);
            await _unitOfWork.YasalFaiz.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logService?.LogError("Yasal faiz güncellenirken hata oluştu.", ex);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.YasalFaiz.GetByIdAsync(id, cancellationToken);
        if (entity == null) return false;

        _unitOfWork.YasalFaiz.Remove(entity);
        await _unitOfWork.YasalFaiz.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> AnyAsync(Expression<Func<YasalFaiz, bool>> predicate, CancellationToken cancellationToken = default)
        => await _unitOfWork.YasalFaiz.AnyAsync(predicate, cancellationToken);
}