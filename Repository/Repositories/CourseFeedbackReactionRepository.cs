using BusinessObject;
using BusinessObject.Model;
using Microsoft.EntityFrameworkCore;
using Repository.BaseRepository;
using Repository.IRepositories;

namespace Repository.Repositories
{
    public class CourseFeedbackReactionRepository : BaseRepository<CourseFeedbackReaction>, ICourseFeedbackReactionRepository
    {
        private readonly IGCSEContext _context;

        public CourseFeedbackReactionRepository(IGCSEContext context) : base(context)
        {
            _context = context;
        }

        public async Task<CourseFeedbackReaction?> GetByFeedbackAndUserAsync(int courseFeedbackId, string userId)
        {
            return await _context.Set<CourseFeedbackReaction>()
                .FirstOrDefaultAsync(r => r.CourseFeedbackId == courseFeedbackId && r.UserId == userId);
        }

        public async Task<int> GetLikeCountAsync(int courseFeedbackId)
        {
            return await _context.Set<CourseFeedbackReaction>()
                .CountAsync(r => r.CourseFeedbackId == courseFeedbackId && r.ReactionType == "Like");
        }

        public async Task<int> GetUnlikeCountAsync(int courseFeedbackId)
        {
            return await _context.Set<CourseFeedbackReaction>()
                .CountAsync(r => r.CourseFeedbackId == courseFeedbackId && r.ReactionType == "Unlike");
        }

        public async Task<bool> HasUserLikedAsync(int courseFeedbackId, string userId)
        {
            return await _context.Set<CourseFeedbackReaction>()
                .AnyAsync(r => r.CourseFeedbackId == courseFeedbackId && r.UserId == userId && r.ReactionType == "Like");
        }

        public async Task<bool> HasUserUnlikedAsync(int courseFeedbackId, string userId)
        {
            return await _context.Set<CourseFeedbackReaction>()
                .AnyAsync(r => r.CourseFeedbackId == courseFeedbackId && r.UserId == userId && r.ReactionType == "Unlike");
        }
    }
}

