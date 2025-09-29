using BusinessObject.IdentityModel;
using BusinessObject.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Repository.IBaseRepository;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Repository.BaseRepository
{
    public class TokenRepository : ITokenRepository
    {
        private readonly IConfiguration _config;
        private readonly SymmetricSecurityKey _key;
        private readonly UserManager<Account> _userManager;
        private readonly IGCSEContext _dbContext;

        public TokenRepository(IConfiguration config, UserManager<Account> userManager, IGCSEContext dbContext)
        {
            _config = config;
            _userManager = userManager;
            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JWT:SigningKey"]));
            _dbContext = dbContext;
        }

        public async Task<TokenModel> createToken(Account account)
        {
            var user = await _userManager.FindByEmailAsync(account.Email);
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Email, account.Email),
                new Claim(JwtRegisteredClaimNames.Sub, account.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.GivenName, account.UserName),
                new Claim("AccountID", account.Id.ToString())
                //new Claim(JwtRegisteredClaimNames.Sub, account.Id.ToString())
            };

            var userRoles = await _userManager.GetRolesAsync(user);
            foreach (var role in userRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role.ToString()));
            }
            var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = creds,
                Issuer = _config["JWT:Issuer"],
                Audience = _config["JWT:Audience"]
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(tokenDescriptor);


            var accessToken = tokenHandler.WriteToken(token);
            var refreshToken = GenerateRefreshToken();

            // saved into database
            var refreshTokenEntity = new RefreshToken
            {
                Id = Guid.NewGuid(),
                JwtID = token.Id,
                AccountID = account.Id,
                Token = refreshToken,
                IsUsed = false,
                IsRevoked = false,
                CreateAt = DateTime.Now,
                ExpiredAt = DateTime.Now.AddDays(7)
            };

            await _dbContext.AddAsync(refreshTokenEntity);
            await _dbContext.SaveChangesAsync();

            return new TokenModel
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }
        private static string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = new System.Security.Cryptography.RNGCryptoServiceProvider())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
                /*.TrimEnd('=')    // ✅ Remove padding '='
                .Replace('+', '-') // ✅ Convert to Base64 URL encoding
                .Replace('/', '_')*/
            }
        }

        public async Task<ApiResponse> renewToken(TokenModel model)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenValidateParam = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = _config["JWT:Issuer"],
                ValidateAudience = true,
                ValidAudience = _config["JWT:Audience"],
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = _key,
                ValidateLifetime = false
            };

            try
            {
                //Check AccessToken valid or not 
                var tokenInVerification = tokenHandler.ValidateToken(model.AccessToken, tokenValidateParam, out var validatedToken);

                if (validatedToken is not SecurityToken)
                {
                    return await Task.FromResult(new ApiResponse
                    {
                        Success = false,
                        Message = "Validated is not SecurityToken"
                    });
                }
                //Check algorithm
                /*if(validatedToken is JwtSecurityToken jwtSecurityToken)
                {
                    var algorithm = jwtSecurityToken.Header.Alg;
                    if (algorithm == null)
                    {
                        return await Task.FromResult(new ApiResponse
                        {
                            Success = false,
                            Message = "Cannot get a algorithm."
                        });
                    }
                    var result = jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha512Signature, StringComparison.InvariantCultureIgnoreCase);
                    if (result == false)
                    {
                        return await Task.FromResult(new ApiResponse
                        {
                            Success = false,
                            Message = "Invalid token algorithm."
                        });
                    }
                }*/


                // check 3: Check accessToken expire?
                var utcExpireDate = long.Parse(tokenInVerification.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Exp).Value);
                var expireDate = ConvertUnixTimeToDateTime(utcExpireDate);
                if (expireDate > DateTime.UtcNow)
                {
                    return await Task.FromResult(new ApiResponse
                    {
                        Success = false,
                        Message = "Access token has not yet expired"
                    });
                }

                //check 4: Check refreshtoken exist in DB
                var storedToken = _dbContext.RefreshTokens.FirstOrDefault(x => x.Token == model.RefreshToken);
                if (storedToken == null)
                {
                    return await Task.FromResult(new ApiResponse
                    {
                        Success = false,
                        Message = "Refresh token does not exist"
                    });
                }

                //check 5: check refreshToken is used/revoked?
                if (storedToken.IsUsed)
                {
                    return await Task.FromResult(new ApiResponse
                    {
                        Success = false,
                        Message = "Refresh token has been used"
                    });
                }
                if (storedToken.IsRevoked)
                {
                    return await Task.FromResult(new ApiResponse
                    {
                        Success = false,
                        Message = "Refresh token has been revoked"
                    });
                }

                //check 6: AccessToken id == JwtId in RefreshToken
                var jti = tokenInVerification.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti).Value;
                if (storedToken.JwtID != jti)
                {
                    return await Task.FromResult(new ApiResponse
                    {
                        Success = false,
                        Message = "Token doesn't match"
                    });
                }

                //Update token is used
                storedToken.IsRevoked = true;
                storedToken.IsUsed = true;
                _dbContext.Update(storedToken);
                await _dbContext.SaveChangesAsync();

                //create new token
                var user = await _dbContext.Users.SingleOrDefaultAsync(nd => nd.Id == storedToken.AccountID);
                var token = await createToken(user);

                return new ApiResponse
                {
                    Success = true,
                    Message = "Renew token success",
                    Data = token
                };

            }
            catch (Exception ex)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "Something went wrong"
                };
            }

        }
        private DateTime ConvertUnixTimeToDateTime(long utcExpireDate)
        {
            var dateTimeInterval = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTimeInterval.AddSeconds(utcExpireDate).ToUniversalTime();

            return dateTimeInterval;
        }
    }
}
