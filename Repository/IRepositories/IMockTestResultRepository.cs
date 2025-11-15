using BusinessObject.DTOs.Request.MockTest;
using BusinessObject.DTOs.Response.MockTest;
using BusinessObject.Model;
using Repository.IBaseRepository;
using System.Linq.Expressions;

namespace Repository.IRepositories
{
    public interface IMockTestResultRepository : IBaseRepository<Mocktestresult>
    {
        Task<List<MockTestResultQueryResponse>> GetMockTestResultList(MockTestResultQueryRequest request, Expression<Func<Mocktestresult, bool>>? filter = null);
        Task<MockTestResultReviewResponse> GetMockTestResultWithReview(int id);
    }
}
