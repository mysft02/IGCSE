using BusinessObject.Payload.Request;
using BusinessObject.Payload.Request.OpenAI;
using Common.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Service.OpenAI;
using Service.Trello;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Principal;

namespace IGCSE.Controller;

[ApiController]
[Route("api/ping")]
public class PingController : ControllerBase
{
    private readonly TrelloApiService _trelloApiService;
    private readonly OpenAIApiService _openApiService;
    public PingController(TrelloApiService trelloApiService, OpenAIApiService openApiService)
    {
        _trelloApiService = trelloApiService;
        _openApiService = openApiService;
    }

    [HttpGet]
    public IActionResult Ping()
    {
        return Ok(new { message = "pong" });
    }

    [HttpGet("test-token")]
    [Authorize]
    public IActionResult PingToken()
    {
        var user = HttpContext.User;
        var userId = user.FindFirst("AccountID")?.Value;

        return Ok(new { message =  userId});
    }

    // GET api/ping/trello?key=...&token=...
    [HttpGet("trello")]
    [SwaggerOperation(Summary = "Gọi trello")]
    public async Task<IActionResult> PingTrello([FromQuery] string token)
    {
        var request = TrelloApiRequest.Builder()
            .CallUrl("/members/me")
            .TrelloToken(token)
            .BaseType(typeof(object))
            .ResponseType(TrelloApiRequest.ResponseType.Single)
            .Build();

        var result = await _trelloApiService.GetAsync<object>(request);
        return Ok(result);
    }

    [HttpGet("openai")]
    [SwaggerOperation(Summary = "Gọi api bên trello")]
    public async Task<IActionResult> PingOpenAI()
    {
        var apiKey = CommonUtils.GetApiKey("OPEN_API_KEY");

        var request = OpenApiRequest.Builder()
            .CallUrl("/models")
            .Build();
        request.BuildUrl();

        var result = await _openApiService.GetAsync<object>(request);
        return Ok(result);
    }
}