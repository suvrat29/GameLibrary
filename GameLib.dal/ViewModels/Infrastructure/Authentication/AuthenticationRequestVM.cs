namespace GameLib.dal.ViewModels.Infrastructure.Authentication;

public class AuthenticationRequestVM
{
    public required string email { get; set; }
    public required string password { get; set; }
}

public class TokenRefreshRequestVM
{
    public required string access_token { get; set; }
    public required string refresh_token { get; set; }
}