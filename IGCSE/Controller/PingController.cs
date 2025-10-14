using Microsoft.AspNetCore.Mvc;
using BusinessObject.Payload.Request;
using Service.Trello;
using BusinessObject.Payload.Request.OpenAI;
using Common.Utils;
using Service.OpenAI;
using System.Security.Principal;
using Microsoft.AspNetCore.Http.Extensions;

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

    [HttpGet("url")]
    public IActionResult PingUrl()
    {
        var currUrl = $"{Request.GetDisplayUrl()}";

        return Ok(new { message = currUrl });
    }

    // GET api/ping/trello?key=...&token=...
    [HttpGet("trello")]
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
    public async Task<IActionResult> PingOpenAI()
    {
        var apiKey = CommonUtils.GetApiKey("OPEN_API_KEY");

        var request = OpenApiRequest.Builder()
            .CallUrl("https://api.openai.com/v1/models")
            .Build();

        var result = await _openApiService.GetAsync<object>(request);
        return Ok(result);
    }
}