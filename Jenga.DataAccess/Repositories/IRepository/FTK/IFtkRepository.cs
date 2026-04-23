using Jenga.Models.FTK;

namespace Jenga.DataAccess.Repositories.IRepository.FTK
{
    public interface IFtkRepository : IRepository<Ftk>
    {
        /// <summary>
        /// Her FTKIslemId için yalnızca en yüksek Sayac değerine sahip kaydı döner.
        /// SQL: WHERE Sayac = (SELECT MAX(Sayac) FROM FTK_Table WHERE FTKIslemId = A.FTKIslemId)
        /// </summary>
        Task<List<Ftk>> GetLatestPerIslemAsync(CancellationToken cancellationToken = default);
    }
}
