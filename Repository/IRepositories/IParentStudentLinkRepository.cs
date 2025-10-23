using BusinessObject.Model;
using Repository.IBaseRepository;

namespace Repository.IRepositories
{
    public interface IParentStudentLinkRepository : IBaseRepository<Parentstudentlink>
    {
        Task<IEnumerable<Account>> GetByParentId(string id);   
        Task<bool> CheckDuplicateParentStudentLink(string parentId, string studentId);
    }
}
