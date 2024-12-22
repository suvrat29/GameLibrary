namespace GameLib.dal.ViewModels.Infrastructure.Authentication;

public class LoginRequestVM
{
    public required string email { get; set; }
    public required string password { get; set; }
}