using System.Threading.Tasks;
using UserManagementAPI.Models;

namespace UserManagementAPI.Services
{
    public interface ITokenService
    {
        Task<TokenResult> GenerateTokenAsync(User user);
    }

    public record TokenResult(bool IsSuccess, string Token, DateTimeOffset Expires, string ErrorMessage = null)
    {
        public static TokenResult Success(string token, DateTimeOffset expires) => new TokenResult(true, token, expires);
        public static TokenResult Failure(string errorMessage) => new TokenResult(false, null, default, errorMessage);
    }
}
