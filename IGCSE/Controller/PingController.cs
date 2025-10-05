using Microsoft.AspNetCore.Mvc;
using BusinessObject.Payload.Request;
using Service.Trello;

namespace IGCSE.Controller;

[ApiController]
[Route("api/ping")]
public class PingController : ControllerBase
{
    private readonly TrelloApiService _trelloApiService;

    public PingController(TrelloApiService trelloApiService)
    {
        _trelloApiService = trelloApiService;
    }

    [HttpGet]
    public IActionResult Ping()
    {
        return Ok(new { message = "pong" });
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
}