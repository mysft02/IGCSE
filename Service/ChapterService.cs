using BusinessObject.DTOs.Request.Chapters;
using BusinessObject.Model;
using Repository.IRepositories;
using AutoMapper;
using BusinessObject.DTOs.Response.Chapters;

namespace Service
{
    public class ChapterService
    {
        private readonly IChapterRepository _chapterRepository;
        private readonly IMapper _mapper;
        public ChapterService(IChapterRepository chapterRepository, IMapper mapper)
        {
            _chapterRepository = chapterRepository;
            _mapper = mapper;
        }
        public async Task<List<ChapterResponse>> GetByModuleIdAsync(int moduleId)
        {
            var chapters = await _chapterRepository.GetByModuleIdAsync(moduleId);
            return _mapper.Map<List<ChapterResponse>>(chapters);
        }
        public async Task<ChapterResponse?> GetByIdAsync(int chapterId)
        {
            var chapter = await _chapterRepository.GetByIdAsync(chapterId);
            return _mapper.Map<ChapterResponse>(chapter);
        }
        public async Task<ChapterResponse> CreateAsync(ChapterRequest request)
        {
            // Map the request to a Chapter entity
            var chapter = _mapper.Map<Chapter>(request);
            
            // Set timestamps
            chapter.CreatedAt = DateTime.UtcNow;
            chapter.UpdatedAt = DateTime.UtcNow;
            
            // Save to database
            var created = await _chapterRepository.AddAsync(chapter);
            
            // Map the saved entity to the response DTO
            return _mapper.Map<ChapterResponse>(created);
        }
        public async Task<ChapterResponse> UpdateAsync(int chapterId, ChapterRequest request)
        {
            var chapter = await _chapterRepository.GetByIdAsync(chapterId);
            if (chapter == null) throw new System.Exception("Chapter not found");
            chapter.ChapterName = request.ChapterName;
            chapter.ChapterDescription = request.ChapterDescription;
            chapter.UpdatedAt = System.DateTime.UtcNow;
            var updated = await _chapterRepository.UpdateAsync(chapter);
            return _mapper.Map<ChapterResponse>(updated);
        }
        public async Task DeleteAsync(int chapterId)
        {
            await _chapterRepository.DeleteAsync(chapterId);
        }
    }
}
