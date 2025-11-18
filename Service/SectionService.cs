using BusinessObject.Model;
using BusinessObject.Payload.Response.Trello;
using Repository.IRepositories;

namespace Service;

public class SectionService
{
    private readonly ICoursesectionRepository _courseSectionRepository;
    
    public SectionService(ICoursesectionRepository courseSectionRepository)
    {
        _courseSectionRepository = courseSectionRepository;
    }
    
    public async Task<Coursesection> CreateCourseSectionForTrelloAsync(int courseId, string sectionName, int sectionOrder, List<TrelloCardResponse> trelloCardResponses)
    {
        sectionName = sectionName.Replace("[section]", "").Trim();
        string description = "This is a section imported from Trello.";
        foreach (var trelloCardResponse in trelloCardResponses)
        {
            if (trelloCardResponse.Name.Contains("Description"))
            {
                description = trelloCardResponse.Description;
            }
        }
        Coursesection courseSection = new Coursesection
        {
            CourseId = courseId,
            Name = sectionName,
            Description = description,
            Order = sectionOrder,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
            IsActive = 1
        };
        var createdSection = await _courseSectionRepository.AddAsync(courseSection);
        return createdSection;
    }
}