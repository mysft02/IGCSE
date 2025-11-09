using BusinessObject.Model;
using BusinessObject.Payload.Response.Trello;
using Common.Constants;
using Repository.IRepositories;
using Repository.Repositories;
using Service.Trello;

namespace Service
{
    public class MockTestQuestionService
    {
        private readonly IMockTestQuestionRepository _mockTestQuestionRepository;
        private readonly TrelloCardService _trelloCardService;

        public MockTestQuestionService(IMockTestQuestionRepository mockTestQuestionRepository, TrelloCardService trelloCardService)
        {
            _mockTestQuestionRepository = mockTestQuestionRepository;
            _trelloCardService = trelloCardService;
        }

        public async Task<Mocktestquestion> CreateMockTestQuestionForTrelloAsync(int mockTestId, string listName, List<TrelloCardResponse> trelloCards, TrelloToken trelloToken)
        {
            var questionContent = listName;
            questionContent = questionContent.Split(':')[1].Trim();

            string correctAnswer = string.Empty;
            string? partialMark = null;
            decimal? mark = null;
            string? imageUrl = null;

            foreach (var card in trelloCards)
            {
                if (card.Name.Contains("Answer:"))
                {
                    var parts = card.Name.Split(':', 2);
                    correctAnswer = parts.Length > 1 ? parts[1].Trim() : card.Name.Trim();
                }
                else if (card.Name.Contains("Marks:") && !card.Name.Contains("Partial"))
                {
                    var parts = card.Name.Split(':', 2);
                    mark = Convert.ToDecimal(parts.Length > 1 ? parts[1].Trim() : card.Name.Trim());
                }
                else if (card.Name.Contains("Partial Marks:"))
                {
                    var parts = card.Name.Split(':', 2);
                    partialMark = parts.Length > 1 ? parts[1].Trim() : card.Name.Trim();
                }
                else if (card.Name.Contains("Image:"))
                {
                    // Lấy attachment đầu tiên của card và tải về wwwroot
                    var attachments = await _trelloCardService.GetTrelloCardAttachments(card.Id, trelloToken);
                    var first = attachments.FirstOrDefault();
                    if (first != null)
                    {
                        imageUrl = await _trelloCardService.DownloadTrelloCardAttachment(first.Url, trelloToken);
                    }
                }
            }

            var question = new Mocktestquestion
            {
                MockTestId = mockTestId,
                QuestionContent = questionContent,
                CorrectAnswer = correctAnswer,
                ImageUrl = imageUrl,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                PartialMark = partialMark,
                Mark = mark
            };

            var createdQuestion = await _mockTestQuestionRepository.AddAsync(question);
            return createdQuestion;
        }
    }
}
