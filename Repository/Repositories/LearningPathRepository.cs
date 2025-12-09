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

        public async Task<Learningpath> GetMatchLearningPath(List<int> moduleList)
        {
            if(moduleList == null || moduleList.Count() == 0)
            {
                return await _context.LearningPaths.FirstOrDefaultAsync();
            }

            var moduleCount = _context.LearningPaths
                .AsEnumerable()
                .Select(x => new
                {
                    x.LearningPathId,
                    x.ModulePath,
                    MatchCount = x.ModulePath.TrimEnd('_').Split('_').Select(int.Parse).ToList().Count(n => moduleList.Contains(n))
                })
                .OrderByDescending(x => x.MatchCount)
                .FirstOrDefault();

            var result = await _context.LearningPaths
                .FirstOrDefaultAsync(x => x.LearningPathId == moduleCount.LearningPathId);

            return result;
        }
    }
}
