using Jenga.Models.IKYS;
using Jenga.Utility.Results;

namespace Jenga.DataAccess.Services.IKYS;

public interface ILeaveRequestWorkflow
{
    const int PageSize = 25;

    Task<Result<LeaveRequestList>> ListAsync(bool personnelOffice, LeaveListFilter filter,
        IzinTalep.OnayDurumuTipi? status, int? page = null, CancellationToken cancellationToken = default);
    Task<Result<LeaveRequestDetail>> DetailAsync(int id, bool personnelOffice, CancellationToken cancellationToken = default,
        bool includeDecisionHistory = false);
    Task<Result<List<LeaveOption>>> ListPersonnelForEntryAsync(CancellationToken cancellationToken = default);
    Task<Result<LeaveRequestForm>> LoadFormAsync(int? id, bool personnelOffice, CancellationToken cancellationToken = default);
    Task<Result<List<LeaveRequestRow>>> ListPendingForEntryAsync(LeaveRequestInput input, bool personnelOffice,
        CancellationToken cancellationToken = default);
    Task<Result<string>> GetNursingAddressAsync(int personelId, bool personnelOffice, CancellationToken cancellationToken = default);
    Task<Result<LeaveCalculation>> CalculateAsync(LeaveRequestInput input, bool personnelOffice, CancellationToken cancellationToken = default);
    Task<Result<LeaveWriteResult>> SaveAsync(LeaveRequestInput input, bool personnelOffice, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(int id, bool personnelOffice, CancellationToken cancellationToken = default);
    Task<Result<LeaveWriteResult>> DecideAsync(int id, LeaveDecision decision, string? note, bool documentReceived,
        int? targetPeriodId = null, CancellationToken cancellationToken = default);
}
