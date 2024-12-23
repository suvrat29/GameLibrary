namespace GameLib.dal.ViewModels.Infrastructure.Authentication;

public class AuthenticationResponseVM
{
    public string access_token { get; set; } = "";
    public string token_type { get; set; } = "Bearer";
    public long expires_in { get; set; } = 0;
    public DateTime expires_at { get; set; } = DateTime.MinValue;
    public string refresh_token { get; set; } = "";
}