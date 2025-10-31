using BusinessObject.DTOs.Response;
using BusinessObject.Payload.Request.Filter;
using Common.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service;

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
    [Authorize(Roles = "Teacher")]
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
}