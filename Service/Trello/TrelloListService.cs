using BusinessObject.Model;
using BusinessObject.Payload.Request;
using BusinessObject.Payload.Response.Trello;

namespace Service.Trello;

public class TrelloListService
{
    private readonly TrelloApiService _trelloApiService;
    
    public TrelloListService(TrelloApiService trelloApiService)
    {
        _trelloApiService = trelloApiService;
    }
    
    public async Task<List<TrelloCardResponse>> GetTrelloCardByList(string listId, TrelloToken trelloToken)
    {
        TrelloApiRequest request = TrelloApiRequest.Builder()
            .CallUrl("/lists/{listId}/cards")
            .AddPathVariable("listId", listId)
            .AddParameter("fields", "id,name,desc")
            .BaseType(typeof(TrelloCardResponse))
            .ResponseType(TrelloApiRequest.ResponseType.Search)
            .TrelloToken(trelloToken.TrelloApiToken)
            .Build();
        List<TrelloCardResponse> trelloCards = (await _trelloApiService.GetAsync<TrelloCardResponse[]>(request))?.ToList() ?? new List<TrelloCardResponse>();
        return trelloCards;
    }
}