using BusinessObject.Model;
using BusinessObject.Payload.Response.Trello;
using Common.Constants;
using Repository.IRepositories;
using Service.Trello;

namespace Service;

public class LessonService
{
    private readonly ILessonRepository _lessonRepository;
    private readonly ILessonitemRepository _lessonitemRepository;
    private readonly TrelloCardService _trelloCardService;
    
    public LessonService(ILessonRepository lessonRepository, ILessonitemRepository lessonitemRepository, TrelloCardService trelloCardService)
    {
        _lessonRepository = lessonRepository;
        _lessonitemRepository = lessonitemRepository;
        _trelloCardService = trelloCardService;
    }

    public async Task CreateLessonForTrelloAsync(int sectionId, string lessonName, int lessonOrder, List<TrelloCardResponse> trelloCardResponses, TrelloToken trelloToken)
    {
        lessonName = lessonName.Replace("[lesson]", "").Trim();
        string description = "This is a lesson imported from Trello.";
        List<Lessonitem> lessonItems = new List<Lessonitem>();
        int itemOrder = 1;
        foreach (var trelloCardResponse in trelloCardResponses)
        {
            if (trelloCardResponse.Name.Contains("Description"))
            {
                description = trelloCardResponse.Description;
            }
            else if (trelloCardResponse.Name.Contains("Video"))
            {
                List<TrelloCardAttachmentsResponse> attachments = await _trelloCardService.GetTrelloCardAttachments(trelloCardResponse.Id, trelloToken);

                // get first attachment that is video
                var videoAttachment = attachments.FirstOrDefault();
                // download video
                var videoUrl = await _trelloCardService.DownloadTrelloCardAttachment(videoAttachment.Url, trelloToken);
                
                lessonItems.Add(new Lessonitem
                {
                    Order = itemOrder,
                    Name = trelloCardResponse.Name.Replace("[Video]", "").Trim(),
                    Description = trelloCardResponse.Description,
                    Content = videoUrl,
                    ItemType = LessonItemType.Video,
                });
                itemOrder++;
            }
            else if (trelloCardResponse.Name.Contains("[PDF]"))
            {
                var attachments = await _trelloCardService.GetTrelloCardAttachments(trelloCardResponse.Id, trelloToken);
                
                // get first attachment that is pdf
                var pdfAttachment = attachments.FirstOrDefault();
                var pdfUrl = await _trelloCardService.DownloadTrelloCardAttachment(pdfAttachment.Url, trelloToken);
                lessonItems.Add(new Lessonitem
                {
                    Order = itemOrder,
                    Name = trelloCardResponse.Name.Replace("[Video]", "").Trim(),
                    Description = trelloCardResponse.Description,
                    Content = pdfUrl,
                    ItemType = LessonItemType.Pdf,
                });
                itemOrder++;
            }
            else if (trelloCardResponse.Name.Contains("[Image]"))
            {
                var attachments = await _trelloCardService.GetTrelloCardAttachments(trelloCardResponse.Id, trelloToken);
                
                // get first attachment that is image
                var imageAttachment = attachments.FirstOrDefault();
                var imageUrl = await _trelloCardService.DownloadTrelloCardAttachment(imageAttachment.Url, trelloToken);
                lessonItems.Add(new Lessonitem
                {
                    Order = itemOrder,
                    Name = trelloCardResponse.Name.Replace("[Image]", "").Trim(),
                    Description = trelloCardResponse.Description,
                    Content = imageUrl,
                    ItemType = LessonItemType.Image,
                });
                itemOrder++;
            }
        }
        Lesson lesson = new Lesson
        {
            CourseSectionId = sectionId,
            Name = lessonName,
            Description = description,
            Order = lessonOrder,
            IsActive = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        var createdLesson = await _lessonRepository.AddAsync(lesson);
        foreach (var lessonItem in lessonItems)
        {
            lessonItem.LessonId = createdLesson.LessonId;
            lessonItem.CreatedAt = DateTime.UtcNow;
            lessonItem.UpdatedAt = DateTime.UtcNow;
            await _lessonitemRepository.AddAsync(lessonItem);
        }
    }
}