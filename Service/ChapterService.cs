using BusinessObject.DTOs.Request.Chapters;
using BusinessObject.DTOs.Response.Chapters;
using BusinessObject.Model;
using Repository.IRepositories;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;

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
            var chapter = _mapper.Map<Chapter>(request);
            chapter.CreatedAt = System.DateTime.UtcNow;
            chapter.UpdatedAt = System.DateTime.UtcNow;
            var created = await _chapterRepository.AddAsync(chapter);
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
