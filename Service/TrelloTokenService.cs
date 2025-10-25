using BusinessObject.DTOs.Response;
using BusinessObject.Model;
using BusinessObject.Payload.Request.Filter;
using Repository.IRepositories;
using AutoMapper;

namespace Service;

public class TrelloTokenService
{
    private readonly ITrelloTokenRepository _trelloTokenRepository;
    private readonly IMapper _mapper;

    public TrelloTokenService(ITrelloTokenRepository trelloTokenRepository, IMapper mapper)
    {
        _trelloTokenRepository = trelloTokenRepository;
        _mapper = mapper;
    }

    /// <summary>
    /// Search Trello tokens with filtering and pagination
    /// </summary>
    public async Task<PaginatedResponse<TrelloTokenResponse>> SearchTrelloTokensAsync(TrelloTokenQueryRequest request)
    {
        // Build filter expression
        var filter = request.BuildFilter<TrelloToken>();
        
        // Get total count first (for pagination info)
        var totalCount = await _trelloTokenRepository.CountAsync(filter);
        
        // Get filtered data with pagination
        var items = await _trelloTokenRepository.FindWithPagingAsync(
            filter, 
            request.Page, 
            request.GetPageSize()
        );

        // Apply sorting to the paged results
        var sortedItems = request.ApplySorting(items);
        
        var totalPages = (int)Math.Ceiling((double)totalCount / request.GetPageSize());

        // Map to response
        return new PaginatedResponse<TrelloTokenResponse>
        {
            Items = sortedItems.Select(token => _mapper.Map<TrelloTokenResponse>(token)).ToList(),
            TotalCount = totalCount,
            Page = request.Page,
            Size = request.GetPageSize(),
            TotalPages = totalPages
        };
    }
}
