using Jenga.Models.IKYS;
using Jenga.Utility.Results;

namespace Jenga.DataAccess.Services.IKYS;

public interface IGorevOnayService
{
    Task<Result<List<GorevOnay>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<List<GorevOnay>>> GetByPersonelIdAsync(int personelId, CancellationToken cancellationToken = default);
    Task<Result<GorevOnay>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Result> AddAsync(GorevOnay entity, string? modifiedBy = null, CancellationToken cancellationToken = default);
    Task<Result> UpdateAsync(GorevOnay entity, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(GorevOnay entity, CancellationToken cancellationToken = default);

    /// <summary>Returns pending approval items scoped to units where the given personel is the manager.</summary>
    Task<Result<List<PendingApprovalItem>>> GetPendingByManagerAsync(int managerPersonelId, CancellationToken cancellationToken = default);

    /// <summary>Approves a GorevOnay record and sends notification email.</summary>
    Task<Result> ApproveAsync(int gorevOnayId, int managerPersonelId, string? approvedBy = null, CancellationToken cancellationToken = default);

    /// <summary>Rejects a GorevOnay record with a reason and sends notification email.</summary>
    Task<Result> RejectAsync(int gorevOnayId, int managerPersonelId, string rejectReason, string? rejectedBy = null, CancellationToken cancellationToken = default);

    Task<Result<List<TaskApprovalListItem>>> GetTaskApprovalListAsync(
        int? currentPersonelId,
        bool authorizedUnitView,
        CancellationToken cancellationToken = default);

    Task<Result<List<TaskApprovalListItem>>> GetTaskApprovalReportItemsAsync(
        IReadOnlyCollection<int> gorevOnayIds,
        int? currentPersonelId,
        bool authorizedUnitView,
        CancellationToken cancellationToken = default);

    Task<Result<GorevOnay>> GetScopedByIdAsync(
        int id,
        int? currentPersonelId,
        bool authorizedUnitView,
        CancellationToken cancellationToken = default);

    Task<Result<List<Personel>>> GetManagedPersonnelAsync(
        int managerPersonelId,
        CancellationToken cancellationToken = default);

    Task<Result> AddScopedAsync(
        GorevOnay entity,
        int? currentPersonelId,
        bool authorizedUnitView,
        string? modifiedBy = null,
        CancellationToken cancellationToken = default);

    Task<Result> UpdateScopedAsync(
        GorevOnay entity,
        int? currentPersonelId,
        bool authorizedUnitView,
        CancellationToken cancellationToken = default);
}
