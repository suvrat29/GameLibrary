namespace GameLib.dal.ViewModels.Infrastructure;

public class BasePatchResponse<T>
{
    public T? Original { get; set; }
    public T? Patched { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; } = "";
}