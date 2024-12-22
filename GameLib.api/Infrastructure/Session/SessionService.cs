using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using GameLib.dal.ViewModels.Infrastructure;

namespace GameLib.api.Infrastructure.Session;

public interface ISessionService
{
    UserModel? GetUser();
}

internal sealed class SessionService(IHttpContextAccessor context) : ISessionService
{
    #region Variables
    private readonly IHttpContextAccessor _context = context;
    #endregion

    #region ISessionService Implementation
    public UserModel? GetUser()
    {
        if (Convert.ToBoolean(_context.HttpContext?.User.Identity?.IsAuthenticated))
        {
            if (_context.HttpContext.Request.Headers.ContainsKey("Authorization"))
            {
                if (_context.HttpContext.Request.Headers.TryGetValue("Authorization", out var headerValues))
                {
                    string? authToken = headerValues.FirstOrDefault();

                    if (!string.IsNullOrWhiteSpace(authToken))
                    {
                        string jwtToken = authToken.Split("Bearer").Last().Trim();
                        if (ValidateJwt(jwtToken))
                        {
                            (string, Guid) userDetails = DecodeJwtToken(BuildJwtSecurityToken(jwtToken));

                            if (!string.IsNullOrWhiteSpace(userDetails.Item1) && userDetails.Item2 != Guid.Empty)
                            {
                                return new ()
                                {
                                    Email = userDetails.Item1,
                                    Uuid = userDetails.Item2
                                };
                            }
                        }
                    }
                }
            }
        }
        return null;
    }
    #endregion
    
    #region Private Methods

    private bool ValidateJwt(string token)
    {
        JwtSecurityTokenHandler handler = new();
        if (!handler.CanReadToken(token))
        {
            return false;
        }
        return true;
    }

    private JwtSecurityToken BuildJwtSecurityToken(string token)
    {
        JwtSecurityTokenHandler handler = new();
        return handler.ReadJwtToken(token);
    }

    private (string, Guid) DecodeJwtToken(JwtSecurityToken jwtSecurityToken)
    {
        List<Claim> claims = jwtSecurityToken.Claims.ToList();
        if (!claims.Any())
        {
            return ("", Guid.Empty);
        }

        string email = claims.FirstOrDefault(c => c.Type.Equals("email", StringComparison.InvariantCultureIgnoreCase))?.Value ?? "";
        string uuid = claims.FirstOrDefault(c => c.Type.Equals("sub", StringComparison.InvariantCultureIgnoreCase))?.Value ?? "";

        if (!string.IsNullOrWhiteSpace(email) && !string.IsNullOrWhiteSpace(uuid))
        {
            if (Guid.TryParse(uuid, out Guid userUuid))
            {
                return (email, userUuid);    
            }
        }
        
        return ("", Guid.Empty);
    }
    #endregion
}