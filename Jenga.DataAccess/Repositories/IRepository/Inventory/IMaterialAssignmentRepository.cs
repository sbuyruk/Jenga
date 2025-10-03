using Jenga.Models.Inventory;

namespace Jenga.DataAccess.Repositories.IRepository.Inventory
{
    public interface IMaterialAssignmentRepository : IRepository<MaterialAssignment>
    {
        Task<List<MaterialAssignment>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<MaterialAssignment?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task AddAsync(MaterialAssignment assignment, CancellationToken cancellationToken = default);
    }
}