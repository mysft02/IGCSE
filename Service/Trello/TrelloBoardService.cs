using BusinessObject.Model;
using BusinessObject.Payload.Request;
using BusinessObject.Payload.Response.Trello;

namespace Service.Trello;

public class TrelloBoardService
{
    private readonly TrelloApiService _trelloApiService;
    
    public TrelloBoardService(TrelloApiService trelloApiService)
    {
        _trelloApiService = trelloApiService;
    }
    
    public async Task<List<TrelloBoardResponse>> GetTrelloBoard(TrelloToken trelloToken)
    {
        TrelloApiRequest request = TrelloApiRequest.Builder()
            .CallUrl("/members/me/boards")
            .BaseType(typeof(TrelloAccountResponse))
            .ResponseType(TrelloApiRequest.ResponseType.Search)
            .TrelloToken(trelloToken.TrelloApiToken)
            .Build();
        List<TrelloBoardResponse> trelloBoard = (await _trelloApiService.GetAsync<TrelloBoardResponse[]>(request))?.ToList() ?? new List<TrelloBoardResponse>();
        return trelloBoard;
    }

    public async Task<List<TrelloListResponse>> GetTrelloLists(string boardId, TrelloToken trelloToken)
    {
        TrelloApiRequest request = TrelloApiRequest.Builder()
            .CallUrl("/boards/{boardId}/lists")
            .AddPathVariable("boardId", boardId)
            .BaseType(typeof(TrelloListResponse))
            .ResponseType(TrelloApiRequest.ResponseType.Search)
            .TrelloToken(trelloToken.TrelloApiToken)
            .Build();
        List<TrelloListResponse> trelloLists =
            (await _trelloApiService.GetAsync<TrelloListResponse[]>(request))?.ToList() ??
            new List<TrelloListResponse>();
        return trelloLists;
    }
}