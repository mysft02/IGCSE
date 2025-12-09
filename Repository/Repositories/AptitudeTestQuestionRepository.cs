using BusinessObject;
using BusinessObject.Model;
using Microsoft.EntityFrameworkCore;
using Repository.BaseRepository;
using Repository.IRepositories;

namespace Repository.Repositories
{
    public class AptitudeTestQuestionRepository : BaseRepository<Aptitudetestquestion>, IAptitudeTestQuestionRepository
    {
        private readonly IGCSEContext _context;

        public AptitudeTestQuestionRepository(IGCSEContext context) : base(context)
        {
            _context = context;
        }
    }
}
