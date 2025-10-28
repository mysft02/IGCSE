using BusinessObject.Model;
using BusinessObject.Payload.Request;
using BusinessObject.Payload.Response.Trello;
using Common.Utils;
using Repository.IRepositories;
using Service.Trello;

namespace Service.OAuth;

public class TrelloOAuthService
{
    private readonly TrelloApiService _trelloApiService;
    private readonly ITrelloTokenRepository _trelloTokenRepository;

    public TrelloOAuthService(TrelloApiService trelloApiService, ITrelloTokenRepository trelloTokenRepository)
    {
        _trelloApiService = trelloApiService;
        _trelloTokenRepository = trelloTokenRepository;
    }

    public string connectTrello()
    {
        TrelloApiRequest request = TrelloApiRequest.Builder()
            .CallUrl("authorize").Build();
        request.AddParameter("callback_method", "fragment");
        request.AddParameter("return_url", "http://localhost:5173/trello/callback");
        request.AddParameter("scope", "read,write");
        request.AddParameter("expiration", "never");
        request.AddParameter("response_type", "code");
        return _trelloApiService.PrepareRequest(request);
    }
    
    public async Task<TrelloAccountResponse> GetTrelloUserInfo(string token)
    {
        TrelloApiRequest request = TrelloApiRequest.Builder()
            .CallUrl("/members/me")
            .BaseType(typeof(TrelloAccountResponse))
            .ResponseType(TrelloApiRequest.ResponseType.Single)
            .TrelloToken(token)
            .Build();
        
        var trelloUserInfo = await _trelloApiService.GetAsync<TrelloAccountResponse>(request);
        
        if (CommonUtils.isEmtyObject(trelloUserInfo))
        {
            throw new Exception("Lấy thông tin người dùng Trello thất bại");
        }
        
        return trelloUserInfo;
    }
    
    public async Task callbackTrello(string userId, string token)
    {
        var trelloUserInfo = await GetTrelloUserInfo(token);

        var trelloEntity = new TrelloToken
        {
            UserId = userId,
            TrelloId = trelloUserInfo.Id,
            TrelloApiToken = token,
            Name = trelloUserInfo.FullName ?? trelloUserInfo.Username ?? "Trello User",
            IsSync = false
        };

        await _trelloTokenRepository.AddOrUpdateAsync(trelloEntity, t => new object[] { t.TrelloId, t.UserId });
    }
}