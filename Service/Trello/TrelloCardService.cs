using BusinessObject.Model;
using BusinessObject.Payload.Request;
using BusinessObject.Payload.Response.Trello;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Common.Utils;

namespace Service.Trello;

public class TrelloCardService
{
    private readonly TrelloApiService _trelloApiService;
    private readonly IWebHostEnvironment _webHostEnvironment;
    
    public TrelloCardService(TrelloApiService trelloApiService, IWebHostEnvironment webHostEnvironment)
    {
        _trelloApiService = trelloApiService;
        _webHostEnvironment = webHostEnvironment;
    }

    public async Task<List<TrelloCardAttachmentsResponse>> GetTrelloCardAttachments(string cardId, TrelloToken trelloToken)
    {
        TrelloApiRequest request = TrelloApiRequest.Builder()
            .CallUrl("/cards/{cardId}/attachments")
            .BaseType(typeof(TrelloCardAttachmentsResponse))
            .ResponseType(TrelloApiRequest.ResponseType.Search)
            .TrelloToken(trelloToken.TrelloApiToken)
            .AddPathVariable("cardId", cardId)
            .AddParameter("fields", "url")
            .Build();
        List<TrelloCardAttachmentsResponse> trelloCardAttachments = (await _trelloApiService.GetAsync<TrelloCardAttachmentsResponse[]>(request))?.ToList() ?? new List<TrelloCardAttachmentsResponse>();
        return trelloCardAttachments;
    }
    
    /// <summary>
    /// Download file từ Trello attachment URL và lưu vào server sử dụng FileUploadHelper
    /// </summary>
    /// <param name="url">URL của attachment từ Trello</param>
    /// <param name="trelloToken">Trello token (không sử dụng nhưng giữ để tương thích API)</param>
    /// <returns>Relative URL path của file đã lưu</returns>
    public async Task<string> DownloadTrelloCardAttachment(string url, TrelloToken trelloToken)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            throw new ArgumentException("File URL cannot be null or empty", nameof(url));
        }

        var webRootPath = _webHostEnvironment.WebRootPath ?? _webHostEnvironment.ContentRootPath;

        // Download file từ URL và convert thành IFormFile
        var formFile = await _trelloApiService.DownloadToFormFileAsync(url, trelloToken);

        // Xác định loại file và sử dụng FileUploadHelper tương ứng
        if (FileUploadHelper.IsValidImageFile(formFile))
        {
            return await FileUploadHelper.UploadCourseImageAsync(formFile, webRootPath);
        }
        else if (FileUploadHelper.IsValidLessonDocument(formFile))
        {
            return await FileUploadHelper.UploadLessonDocumentAsync(formFile, webRootPath);
        }
        else if (FileUploadHelper.IsValidLessonVideo(formFile))
        {
            return await FileUploadHelper.UploadLessonVideoAsync(formFile, webRootPath);
        }
        else
        {
            throw new ArgumentException($"File type not supported. URL: {url}");
        }
    }


}