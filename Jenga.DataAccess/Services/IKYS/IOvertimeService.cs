using Jenga.Models.IKYS;
using Jenga.Utility.Results;

namespace Jenga.DataAccess.Services.IKYS;

public interface IOvertimeService
{
    Task<Result<bool>> SetExternalInstitutionAsync(int personelId, string institution, CancellationToken cancellationToken = default);
    Task<Result<OvertimeWorkspace>> LoadAsync(string scope, DateTime start, DateTime end, CancellationToken cancellationToken = default);
    Task<Result<OvertimeDelegationAdminData>> GetDelegationsAsync(CancellationToken cancellationToken = default);
    Task<Result<bool>> SetDelegationsAsync(int delegatePersonelId, List<int> targetPersonelIds, List<int> expectedTargetPersonelIds, CancellationToken cancellationToken = default);
    Task<Result<int>> SaveEntryAsync(OvertimeEntryInput input, CancellationToken cancellationToken = default);
    Task<Result<List<int>>> SaveEntriesAsync(OvertimeEntryInput input, CancellationToken cancellationToken = default);
    Task<Result<bool>> WithdrawAsync(int entryId, byte[] version, CancellationToken cancellationToken = default);
    Task<Result<bool>> ReviewAsync(int entryId, byte[] version, OvertimeEntryStatus decision, string reason, CancellationToken cancellationToken = default);
    Task<Result<List<OvertimeEvent>>> GetEntryHistoryAsync(int entryId, CancellationToken cancellationToken = default);
    Task<Result<int>> CreatePeriodAsync(string name, DateTime start, DateTime end, DateTime dueDate, CancellationToken cancellationToken = default);
    Task<Result<bool>> IssueUnitAsync(int deliveryId, byte[] version, CancellationToken cancellationToken = default);
    Task<Result<bool>> ReceiveUnitAsync(int deliveryId, byte[] version, CancellationToken cancellationToken = default);
    Task<Result<bool>> ReopenUnitAsync(int deliveryId, byte[] version, string reason, CancellationToken cancellationToken = default);
    Task<Result<bool>> PrepareSignatureAsync(int periodId, byte[] version, CancellationToken cancellationToken = default);
    Task<Result<bool>> ReopenPeriodAsync(int periodId, byte[] version, string reason, CancellationToken cancellationToken = default);
    Task<Result<bool>> FinalizeAsync(int periodId, byte[] version, string fileName, byte[] document, CancellationToken cancellationToken = default);
    Task<Result<bool>> SetScheduleAsync(int titleId, int weeklyHours, CancellationToken cancellationToken = default);
    Task<Result<OvertimeReport>> GetReportAsync(int periodId, int? unitId, int? personelId, CancellationToken cancellationToken = default);
    Task<Result<(string Name, string ContentType, byte[] Data)>> GetSignedDocumentAsync(int periodId, CancellationToken cancellationToken = default);
}
