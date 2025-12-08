using BusinessObject;
using BusinessObject.Model;
using Microsoft.EntityFrameworkCore;
using Repository.BaseRepository;
using Repository.IRepositories;

namespace Repository.Repositories
{
    public class LearningPathRepository : BaseRepository<Learningpath>, ILearningPathRepository
    {
        private readonly IGCSEContext _context;

        public LearningPathRepository(IGCSEContext context) : base(context)
        {
            _context = context;
        }
    }
}
