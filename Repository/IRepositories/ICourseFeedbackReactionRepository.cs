using BusinessObject.Model;
using Repository.IBaseRepository;

namespace Repository.IRepositories
{
    public interface ICourseFeedbackReactionRepository : IBaseRepository<CourseFeedbackReaction>
    {
        Task<CourseFeedbackReaction?> GetByFeedbackAndUserAsync(int courseFeedbackId, string userId);
        Task<int> GetLikeCountAsync(int courseFeedbackId);
        Task<int> GetUnlikeCountAsync(int courseFeedbackId);
        Task<bool> HasUserLikedAsync(int courseFeedbackId, string userId);
        Task<bool> HasUserUnlikedAsync(int courseFeedbackId, string userId);
    }
}

