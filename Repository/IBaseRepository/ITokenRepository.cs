using BusinessObject.IdentityModel;
using BusinessObject.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.IBaseRepository
{
    public interface ITokenRepository
    {
        public Task<TokenModel> createToken(Account application);
        public Task<ApiResponse> renewToken(TokenModel model);

    }
}
