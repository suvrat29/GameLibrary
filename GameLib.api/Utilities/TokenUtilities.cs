﻿using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace GameLib.api.Utilities;

public static class TokenUtilities
{
    #region Shared Methods

    /// <summary>
    /// Gets user email and uuid from the request object.
    /// </summary>
    /// <param name="context">IHttpContextAccessor instance.</param>
    /// <returns>User's email and uuid if successful.</returns>
    public static (string, Guid) GetUserDetailsFromRequest(IHttpContextAccessor context)
    {
        if (context.HttpContext!.Request.Headers.ContainsKey("Authorization"))
        {
            if (context.HttpContext.Request.Headers.TryGetValue("Authorization", out var headerValues))
            {
                string? authToken = headerValues.FirstOrDefault();

                if (!string.IsNullOrWhiteSpace(authToken))
                {
                    string jwtToken = authToken.Split("Bearer").Last().Trim();

                    if (ValidateJwt(jwtToken))
                    {
                        (string, Guid) userDetails = GetUserDataFromRequest(BuildJwtSecurityToken(jwtToken));

                        if (!string.IsNullOrWhiteSpace(userDetails.Item1) && userDetails.Item2 != Guid.Empty)
                        {
                            return userDetails;
                        }
                    }
                }
            }
        }

        return ("", Guid.Empty);
    }

    /// <summary>
    /// Gets user's uuid from the request object.
    /// </summary>
    /// <param name="context">IHttpContextAccessor instance.</param>
    /// <returns>User's uuid if successful.</returns>
    public static Guid GetUserUuidFromRequest(IHttpContextAccessor context)
    {
        if (context.HttpContext!.Request.Headers.ContainsKey("Authorization"))
        {
            if (context.HttpContext.Request.Headers.TryGetValue("Authorization", out var headerValues))
            {
                string? authToken = headerValues.FirstOrDefault();

                if (!string.IsNullOrWhiteSpace(authToken))
                {
                    string jwtToken = authToken.Split("Bearer").Last().Trim();

                    if (ValidateJwt(jwtToken))
                    {
                        (string, Guid) userDetails = GetUserDataFromRequest(BuildJwtSecurityToken(jwtToken));

                        if (!string.IsNullOrWhiteSpace(userDetails.Item1) && userDetails.Item2 != Guid.Empty)
                        {
                            return userDetails.Item2;
                        }
                    }
                }
            }
        }

        return Guid.Empty;
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Reads the JWT token and returns user related data from it.
    /// </summary>
    /// <param name="jwtSecurityToken">The token object.</param>
    /// <returns>User's email and uuid.</returns>
    private static (string, Guid) GetUserDataFromRequest(JwtSecurityToken jwtSecurityToken)
    {
        List<Claim> claims = jwtSecurityToken.Claims.ToList();
        if (!claims.Any())
        {
            return ("", Guid.Empty);
        }

        string email = claims.FirstOrDefault(c => c.Type.Equals("email", StringComparison.InvariantCultureIgnoreCase))
            ?.Value ?? "";
        string uuid = claims.FirstOrDefault(c => c.Type.Equals("sub", StringComparison.InvariantCultureIgnoreCase))
            ?.Value ?? "";

        if (!string.IsNullOrWhiteSpace(email) && !string.IsNullOrWhiteSpace(uuid))
        {
            if (Guid.TryParse(uuid, out Guid userUuid))
            {
                return (email, userUuid);
            }
        }

        return ("", Guid.Empty);
    }

    /// <summary>
    /// Only checks if the JWT is well formed. No checks for validity, etc.
    /// </summary>
    /// <param name="token">The token to be validated.</param>
    /// <returns>Boolean result of the token validation for the specified parameters.</returns>
    private static bool ValidateJwt(string token)
    {
        JwtSecurityTokenHandler handler = new();
        if (!handler.CanReadToken(token))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Transforms a JWT token string into a JwtSecurityToken object.
    /// </summary>
    /// <param name="token">The jwt token in string format.</param>
    /// <returns>JwtSecurityToken object built from reading the token.</returns>
    private static JwtSecurityToken BuildJwtSecurityToken(string token)
    {
        JwtSecurityTokenHandler handler = new();
        return handler.ReadJwtToken(token);
    }

    #endregion
}