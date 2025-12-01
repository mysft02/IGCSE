using BusinessObject.DTOs.Response;
using BusinessObject.DTOs.Response.Trellos;
using BusinessObject.Model;
using BusinessObject.Payload.Request.Filter;
using Common.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service;
using Service.OAuth;
using Swashbuckle.AspNetCore.Annotations;

namespace IGCSE.Controller;

[ApiController]
[Route("api/trellos")]
[Authorize]
public class TrelloController : ControllerBase
{
    private readonly TrelloTokenService _trelloTokenService;

    public TrelloController(TrelloTokenService trelloTokenService)
    {
        _trelloTokenService = trelloTokenService;
    }

    /// <summary>
    /// Search Trello tokens with filtering and pagination
    /// </summary>
    [HttpGet("search")]
    [Authorize(Roles = "Teacher, Manager")]
    public async Task<IActionResult> SearchTrelloTokens([FromQuery] TrelloTokenQueryRequest request)
    {
        var userId = HttpContext.User.FindFirst("AccountID")?.Value;
        if (CommonUtils.IsEmptyString(userId))
        { 
            throw new Exception("Không tìm thấy thông tin người dùng");
        }
                
        // Set current user to the request
        request.userID = userId;

        // Use service to search tokens
        var response = await _trelloTokenService.SearchTrelloTokensAsync(request);
        return Ok(BaseResponse<PaginatedResponse<TrelloTokenResponse>>.Success(response, "Lấy danh sách Trello tokens thành công"));
    }
    
    [HttpGet("{id}/boards")]
    [Authorize(Roles = "Teacher, Manager")]
    public async Task<IActionResult> GetTrelloBoardsByUserId([FromRoute] string id)
    {
        var userId = HttpContext.User.FindFirst("AccountID")?.Value;
        if (CommonUtils.IsEmptyString(userId))
        { 
            throw new Exception("Không tìm thấy thông tin người dùng");
        }
        var trelloBoards = await _trelloTokenService.GetTrelloBoardByUserIdAsync(userId, id);
        return Ok(BaseResponse<List<TrelloBoardDtoResponse>>.Success(trelloBoards, "Lấy danh sách bảng Trello thành công"));
    }

    
    [HttpPost("{id}/boards/{boardId}")]
    [Authorize(Roles = "Teacher, Manager")]
    [SwaggerOperation(
        Summary = "Tiến hành tự động upload khoá học từ Trello",
        Description = "API này cho phép tự động upload dữ liệu khoá học từ một board Trello cụ thể sang hệ thống. " +
                      "Yêu cầu quyền Teacher hoặc Admin. Tham số `id` là ID của khoá học trong hệ thống, " +
                      "`boardId` là ID của board Trello chứa dữ liệu cần upload."
    )]
    public async Task<IActionResult> AutoUploadFromTrello(string id, string boardId)
    {
        var userId = HttpContext.User.FindFirst("AccountID")?.Value;
        if (CommonUtils.IsEmptyString(userId))
        { 
            throw new Exception("Không tìm thấy thông tin người dùng");
        }

        await _trelloTokenService.AutoUploadFromTrelloAsync(userId, id, boardId);
        return Ok(BaseResponse<List<TrelloBoardDtoResponse>>.Success(null, "Đang tiến hành upload bài giảng của bạn"));
    }

    [HttpPost("mock-test/{id}/boards/{boardId}")]
    [Authorize(Roles = "Teacher, Manager")]
    [SwaggerOperation(
        Summary = "Tiến hành tự động upload bài thi thử từ Trello",
        Description = "API này cho phép tự động upload dữ liệu bài thi thử từ một board Trello cụ thể sang hệ thống. " +
                      "Yêu cầu quyền Teacher hoặc Admin. Tham số `id` là ID của bài thi thử trong hệ thống, " +
                      "`boardId` là ID của board Trello chứa dữ liệu cần upload."
    )]
    public async Task<IActionResult> AutoUploadMockTestFromTrello(string id, string boardId)
    {
        var userId = HttpContext.User.FindFirst("AccountID")?.Value;
        if (CommonUtils.IsEmptyString(userId))
        {
            throw new Exception("Không tìm thấy thông tin người dùng");
        }

        await _trelloTokenService.AutoUploadMockTestFromTrelloAsync(userId, id, boardId);
        return Ok(BaseResponse<List<TrelloBoardDtoResponse>>.Success(null, "Đang tiến hành upload bài thi thử của bạn"));
    }
}