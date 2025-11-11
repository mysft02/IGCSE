using BusinessObject;
using BusinessObject.Model;
using Repository.BaseRepository;
using Repository.IRepositories;

namespace Repository.Repositories
{
    public class CreateSlotRepository : BaseRepository<Createslot>, ICreateSlotRepository
    {
        private readonly IGCSEContext _context;

        public CreateSlotRepository(IGCSEContext context) : base(context)
        {
            _context = context;
        }
    }
}
