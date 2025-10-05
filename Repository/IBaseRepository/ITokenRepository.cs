using BusinessObject.IdentityModel;
using BusinessObject.Model;

namespace Repository.IBaseRepository
{
    public interface ITokenRepository
    {
        public Task<TokenModel> createToken(Account application);

    }
}
